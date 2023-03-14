using System.Net;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using DLV_Api;
using DLV_Api.Http;
using Microsoft.VisualBasic.FileIO;
using PlayFab;
using Serilog;

namespace DLV;

[BepInPlugin("org.bepinex.plugins.DLV", "Dreamlight Valley Packet Analyzer", "1.0.0.0")]
// ReSharper disable once UnusedType.Global
public class DlvPacketAnalyzer : BasePlugin
{
    private HttpServer? _httpServer;
    
    public override void Load()
    {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                               Path.Join(SpecialDirectories.MyDocuments, "DLV");

        Serilog.Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Verbose()
#else
            .MinimumLevel.Warning()
#endif
            .WriteTo.Console()
            .WriteTo.File(Path.Join(assemblyLocation, "DLV.log"))
            .CreateLogger();

        var titleId = PlayFabSettings.staticSettings.TitleId;
        if (string.IsNullOrEmpty(titleId))
        {
            titleId = "997EB"; // Title ID for Dreamlight Valley
        }
        
        var productionEnvironmentUrl = PlayFabSettings.ProductionEnvironmentUrl;
        if (string.IsNullOrEmpty(productionEnvironmentUrl))
        {
            productionEnvironmentUrl = "playfabapi.com";
        }

        var listenIp = IPAddress.Parse("127.0.0.1");
        const ushort listenPort = 8080;
        _httpServer = new HttpServer(
            listenIp,
            listenPort,
            titleId,
            productionEnvironmentUrl,
            Path.Join(assemblyLocation, "packets")
        );
        
        // We don't want to block the main thread..
#pragma warning disable CS4014
        _httpServer.Start();
#pragma warning restore CS4014

        PlayFabSettings.ProductionEnvironmentUrl = $"http://{listenIp}:{listenPort}";
    }

    public override bool Unload()
    {
        _httpServer?.Stop();

        return base.Unload();
    }
}