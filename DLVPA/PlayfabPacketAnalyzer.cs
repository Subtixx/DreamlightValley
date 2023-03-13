using System.Net;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Microsoft.VisualBasic.FileIO;
using PlayFab;
using PlayfabApi;
using Serilog;

namespace DLVPA;

[BepInPlugin("org.bepinex.plugins.DLVPA", "Dreamlight Valley Packet Analyzer", "1.0.0.0")]
public class PlayfabPacketAnalyzer : BasePlugin
{
    private HttpServer? _httpServer;

    public override void Load()
    {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                               Path.Join(SpecialDirectories.MyDocuments, "DLVPA");

        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(Path.Join(assemblyLocation, "DLVPA.log"))
            .CreateLogger();

        var titleId = PlayFabSettings.staticSettings.TitleId;
        if (string.IsNullOrEmpty(titleId))
        {
            Serilog.Log.Logger.Error("Title ID is not set");
            return;
        }
        
        var productionEnvironmentUrl = PlayFabSettings.ProductionEnvironmentUrl;
        if (string.IsNullOrEmpty(productionEnvironmentUrl))
        {
            Serilog.Log.Logger.Error("Production environment URL is not set");
            return;
        }

        _httpServer = new HttpServer(
            IPAddress.Parse("127.0.0.1"),
            8080,
            titleId,
            productionEnvironmentUrl,
            Path.Join(assemblyLocation, "packets")
        );
#pragma warning disable CS4014
        _httpServer.Start();
#pragma warning restore CS4014

        PlayFabSettings.ProductionEnvironmentUrl = "http://127.0.0.1:8080";
    }

    public override bool Unload()
    {
        _httpServer?.Stop();

        return base.Unload();
    }
}