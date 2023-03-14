namespace DLV;

public class Core
{
    public const string NAME = "Dreamlight Valley Framework";
    public const string VERSION = "1.0.0";
    public const string AUTHOR = "Subtixx";
    public const string GUID = "de.subtixx.dlv_framework";
    public static HarmonyLib.Harmony Harmony { get; } = new(GUID);
    
    
}