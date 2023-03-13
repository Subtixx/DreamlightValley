using Newtonsoft.Json.Linq;

namespace PlayfabApi.Models;

public class ClientLoginWithSteamPayload
{
    public class AccountInfoClass
    {
        public class TitleInfoClass
        {
            public class TitlePlayerAccountClass
            {
                [Newtonsoft.Json.JsonProperty("Id")]
                public string Id { get; set; } = "";

                [Newtonsoft.Json.JsonProperty("Type")]
                public string Type { get; set; } = "";

                [Newtonsoft.Json.JsonProperty("TypeString")]
                public string TypeString { get; set; } = "";
            }

            [Newtonsoft.Json.JsonProperty("Origination")]
            public string Origination { get; set; } = "";

            [Newtonsoft.Json.JsonProperty("Created")]
            public DateTime Created { get; set; }

            [Newtonsoft.Json.JsonProperty("LastLogin")]
            public DateTime LastLogin { get; set; }

            [Newtonsoft.Json.JsonProperty("FirstLogin")]
            public DateTime FirstLogin { get; set; }

            [Newtonsoft.Json.JsonProperty("isBanned")]
            public bool IsBanned { get; set; }

            [Newtonsoft.Json.JsonProperty("TitlePlayerAccount")]
            public TitlePlayerAccountClass? TitlePlayerAccount { get; set; }
        }

        public class PrivateInfoClass
        {
        }

        public class SteamInfoClass
        {
            [Newtonsoft.Json.JsonProperty("SteamId")]
            public string SteamId { get; set; } = "";

            [Newtonsoft.Json.JsonProperty("SteamName")]
            public string SteamName { get; set; } = "";

            [Newtonsoft.Json.JsonProperty("SteamCountry")]
            public string SteamCountry { get; set; } = "";

            [Newtonsoft.Json.JsonProperty("SteamCurrency")]
            public string SteamCurrency { get; set; } = "";
        }

        public class OpenIdInfoClass
        {
            [Newtonsoft.Json.JsonProperty("ConnectionId")]
            public string ConnectionId { get; set; } = "";

            [Newtonsoft.Json.JsonProperty("Issuer")]
            public string Issuer { get; set; } = "";

            [Newtonsoft.Json.JsonProperty("Subject")]
            public string Subject { get; set; } = "";
        }

        [Newtonsoft.Json.JsonProperty("PlayFabId")]
        public string PlayFabId { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("Created")]
        public DateTime Created { get; set; }

        [Newtonsoft.Json.JsonProperty("TitleInfo")]
        public TitleInfoClass? TitleInfo { get; set; }

        [Newtonsoft.Json.JsonProperty("PrivateInfo")]
        public PrivateInfoClass? PrivateInfo { get; set; }

        [Newtonsoft.Json.JsonProperty("SteamInfo")]
        public SteamInfoClass? SteamInfo { get; set; }

        [Newtonsoft.Json.JsonProperty("OpenIdInfo")]
        public List<OpenIdInfoClass?> OpenIdInfo { get; set; } = new();
    }

    public class UserDataClass
    {
        [Newtonsoft.Json.JsonProperty("Value")]
        public string Value { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("LastUpdated")]
        public DateTime LastUpdated { get; set; }

        [Newtonsoft.Json.JsonProperty("Permission")]
        public string Permission { get; set; } = "";
    }
    
    public class PlayerProfileClass
    {
        public class LocationClass
        {
            [Newtonsoft.Json.JsonProperty("ContinentCode")]
            public string ContinentCode { get; set; } = "";
            
            [Newtonsoft.Json.JsonProperty("CountryCode")]
            public string CountryCode { get; set; } = "";
            
            [Newtonsoft.Json.JsonProperty("City")]
            public string City { get; set; } = "";
            
            [Newtonsoft.Json.JsonProperty("Latitude")]
            public double Latitude { get; set; }
            
            [Newtonsoft.Json.JsonProperty("Longitude")]
            public double Longitude { get; set; }
        }
        
        public class LinkedAccountClass
        {
            [Newtonsoft.Json.JsonProperty("Platform")]
            public string Platform { get; set; } = "";
            
            [Newtonsoft.Json.JsonProperty("PlatformUserId")]
            public string PlatformUserId { get; set; } = "";
            
            [Newtonsoft.Json.JsonProperty("Username")]
            public string Username { get; set; } = "";
        }
        
        public class StatisticClass
        {
            [Newtonsoft.Json.JsonProperty("Name")]
            public string Name { get; set; } = "";
            
            [Newtonsoft.Json.JsonProperty("Version")]
            public int Version { get; set; }
            
            [Newtonsoft.Json.JsonProperty("Value")]
            public int Value { get; set; }
        }
        
        [Newtonsoft.Json.JsonProperty("PublisherId")]
        public string PublisherId { get; set; } = "";
        
        [Newtonsoft.Json.JsonProperty("TitleId")]
        public string TitleId { get; set; } = "";
        
        [Newtonsoft.Json.JsonProperty("PlayerId")]
        public string PlayerId { get; set; } = "";
        
        [Newtonsoft.Json.JsonProperty("Created")]
        public DateTime Created { get; set; }
        
        [Newtonsoft.Json.JsonProperty("LastLogin")]
        public DateTime LastLogin { get; set; }
        
        [Newtonsoft.Json.JsonProperty("Locations")]
        public List<LocationClass> Locations { get; set; } = new();

        [Newtonsoft.Json.JsonProperty("Tags")]
        public List<JObject> Tags { get; set; } = new();
        
        [Newtonsoft.Json.JsonProperty("LinkedAccounts")]
        public List<LinkedAccountClass> LinkedAccounts { get; set; } = new();
        
        [Newtonsoft.Json.JsonProperty("ContactEmailAddresses")]
        public List<JObject> ContactEmailAddresses { get; set; } = new();
        
        [Newtonsoft.Json.JsonProperty("Statistics")]
        public List<StatisticClass> Statistics { get; set; } = new();
    }

    [Newtonsoft.Json.JsonProperty("AccountInfo")]
    public AccountInfoClass AccountInfo { get; set; } = new();

    [Newtonsoft.Json.JsonProperty("UserInventory")]
    public List<JObject> UserInventory { get; set; } = new();

    [Newtonsoft.Json.JsonProperty("UserVirtualCurrency")]
    public Dictionary<string, int> UserVirtualCurrency { get; set; } = new();

    [Newtonsoft.Json.JsonProperty("UserVirtualCurrencyRechargeTimes")]
    public Dictionary<string, JObject> UserVirtualCurrencyRechargeTimes { get; set; } = new();

    [Newtonsoft.Json.JsonProperty("UserData")]
    public Dictionary<string, UserDataClass> UserData { get; set; } = new();

    [Newtonsoft.Json.JsonProperty("UserDataVersion")]
    public int UserDataVersion { get; set; }

    [Newtonsoft.Json.JsonProperty("UserReadOnlyData")]
    public Dictionary<string, UserDataClass> UserReadOnlyData { get; set; } = new();

    [Newtonsoft.Json.JsonProperty("UserReadOnlyDataVersion")]
    public int UserReadOnlyDataVersion { get; set; }

    [Newtonsoft.Json.JsonProperty("CharacterList")]
    public List<JObject> CharacterList { get; set; } = new();

    [Newtonsoft.Json.JsonProperty("CharacterInventories")]
    public List<JObject> CharacterInventories { get; set; } = new();
    
    [Newtonsoft.Json.JsonProperty("TitleData")]
    public Dictionary<string, string> TitleData { get; set; } = new();

    [Newtonsoft.Json.JsonProperty("PlayerStatistics")]
    public List<JObject> PlayerStatistics { get; set; } = new();
    
    [Newtonsoft.Json.JsonProperty("PlayerProfile")]
    public PlayerProfileClass? PlayerProfile { get; set; } = new();
}

public class ClientLoginWithSteam
{
    public class SettingsForUserClass
    {
        [Newtonsoft.Json.JsonProperty("NeedsAttribution")]
        public bool NeedsAttribution { get; set; }

        [Newtonsoft.Json.JsonProperty("GatherDeviceInfo")]
        public bool GatherDeviceInfo { get; set; }

        [Newtonsoft.Json.JsonProperty("GatherFocusInfo")]
        public bool GatherFocusInfo { get; set; }
    }

    public class EntityClass
    {
        [Newtonsoft.Json.JsonProperty("Id")]
        public string Id { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("Type")]
        public string Type { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("TypeString")]
        public string TypeString { get; set; } = "";
    }

    public class EntityTokenClass
    {
        [Newtonsoft.Json.JsonProperty("EntityToken")]
        public string EntityToken { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("TokenExpiration")]
        public DateTime TokenExpiration { get; set; }

        [Newtonsoft.Json.JsonProperty("Entity")]
        public EntityClass? Entity { get; set; }
    }

    public class TreatmentAssignmentClass
    {
        [Newtonsoft.Json.JsonProperty("Variants")]
        public List<JObject> Variants { get; set; } = new();

        [Newtonsoft.Json.JsonProperty("Variables")]
        public List<JObject> Variables { get; set; } = new();
    }

    //UserInventory -> []
    //UserVirtualCurrency -> {}
    //UserVirtualCurrencyRechargeTimes -> {}

    public class UserDataClass
    {
        [Newtonsoft.Json.JsonProperty("Key")]
        public string Key { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("Value")]
        public string Value { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("LastUpdated")]
        public DateTime LastUpdated { get; set; }

        [Newtonsoft.Json.JsonProperty("Permission")]
        public string Permission { get; set; } = "";
    }
    
    [Newtonsoft.Json.JsonProperty("SessionTicket")]
    public string SessionTicket { get; set; } = "";

    [Newtonsoft.Json.JsonProperty("PlayFabId")]
    public string PlayFabId { get; set; } = "";

    [Newtonsoft.Json.JsonProperty("NewlyCreated")]
    public bool NewlyCreated { get; set; }

    [Newtonsoft.Json.JsonProperty("SettingsForUser")]
    public SettingsForUserClass? SettingsForUser { get; set; }

    [Newtonsoft.Json.JsonProperty("LastLoginTime")]
    public DateTime LastLoginTime { get; set; }

    [Newtonsoft.Json.JsonProperty("InfoResultPayload")]
    public ClientLoginWithSteamPayload? InfoResultPayload { get; set; }

    [Newtonsoft.Json.JsonProperty("EntityToken")]
    public EntityTokenClass? EntityToken { get; set; }

    [Newtonsoft.Json.JsonProperty("TreatmentAssignment")]
    public TreatmentAssignmentClass? TreatmentAssignment { get; set; }
}