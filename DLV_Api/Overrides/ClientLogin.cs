using System.Net;
using DLV_Api.Http;
using DLV_Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace DLV_Api.Overrides;

/// <summary>
/// Currently we're overriding the ClientLoginWithSteam endpoint to add the founders pack to the user data.
/// </summary>
// ReSharper disable once UnusedType.Global
public class ClientLogin : ApiRequest
{
    public override string GetPath() => "/Client/LoginWithSteam";

    public override string? AfterRequest(HttpResponse response)
    {
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Body);
        if (apiResponse?.Data == null)
        {
            Log.Error("ClientLoginWithSteam: apiResponse is null");
            return null;
        }

        var clientLoginWithSteam = apiResponse.Data.ToObject<ClientLoginWithSteam>();
        if (clientLoginWithSteam?.InfoResultPayload == null)
        {
            Log.Error("ClientLoginWithSteam: clientLoginWithSteam is null");
            return null;
        }

        clientLoginWithSteam.InfoResultPayload.UserData.Clear();
        clientLoginWithSteam.InfoResultPayload.UserData.Add(
            "awarded_founders_pack", new ClientLoginWithSteamPayload.UserDataClass()
            {
                LastUpdated = new DateTime(),
                Permission = "private",
                // Yes.. There is a typo here. They misspelled awarded 🤦
                Value =
                    "{\"DataVersion\":3,\"AwarwedPacksPerStore\":{\"Steam\":{\"PackPrettyName\":\"FoundersPackUltimate_Steam_OnlineKey\",\"PackInstanceId\":\"" +
                    Guid.NewGuid() + "\"}}}"
            }
        );

        apiResponse.Data = JObject.FromObject(clientLoginWithSteam);

        return JsonConvert.SerializeObject(apiResponse);
    }
}