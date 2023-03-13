using System.Net;
using System.Text;
using Serilog;

namespace PlayfabApi
{
    public class HttpServer
    {
        private static HttpListener listener;

        private static string OriginalUrl = "playfabapi.com";
        public static string TitleId = "C0D0";
        public static string PacketDir = "packets";

        public HttpServer(
            IPAddress listenIp,
            ushort listenPort,
            string titleId,
            string productionEnvironmentUrl,
            string packetDir
        )
        {
            if (listenIp.Equals(IPAddress.Any) || listenIp.Equals(IPAddress.IPv6Any) ||
                listenIp.Equals(IPAddress.None) || listenIp.Equals(IPAddress.IPv6None) ||
                listenIp.Equals(IPAddress.Broadcast) || listenIp.Equals(IPAddress.IPv6Any)
               )
            {
                throw new ArgumentException("Listen IP cannot be a wildcard address");
            }

            TitleId = titleId;
            PacketDir = packetDir;
            OriginalUrl = productionEnvironmentUrl;
            
            var listenIpString = listenIp.ToString();

            listener = new HttpListener();
            var listenPrefix = $"http://{listenIpString}:{listenPort}/";
            listener.Prefixes.Add(listenPrefix);
            Log.Information("HTTP Server will listen on {Prefix}", listenPrefix);

            if (!Directory.Exists(PacketDir))
            {
                Directory.CreateDirectory(PacketDir);
            }
        }

        public async Task Start()
        {
            listener.Start();
            
            Log.Information("HTTP Server started");

            // Handle requests
            await HandleIncomingConnections();
        }

        public void Stop()
        {
            listener.Stop();
        }

        private async static Task HandleIncomingConnections()
        {
            var runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                var ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                var req = ctx.Request;
                var resp = ctx.Response;

                var requestBody = new StreamReader(req.InputStream).ReadToEnd();

                Log.Logger.Debug("Request: {Method} {Url}", req.HttpMethod, req.RawUrl);
                foreach (var header in req.Headers.AllKeys)
                {
                    if (header == null)
                    {
                        continue;
                    }

                    Log.Logger.Debug("Request: {Header} {Value}", header, req.Headers[header]);
                }

                Log.Logger.Debug("Request: {Body}", requestBody);

                Log.Logger.Debug("Request to: {Url}", OriginalUrl + req.RawUrl);

                ServicePointManager.ServerCertificateValidationCallback = delegate
                {
                    return true;
                };

                // Make a request to the original URL
                using (var client = new HttpClient())
                {
                    foreach (var header in req.Headers.AllKeys)
                    {
                        if (header == null || header == "Host" || header == "Content-Length" || header == "Content-Type")
                        {
                            continue;
                        }

                        client.DefaultRequestHeaders.Add(header, req.Headers[header]);
                    }

                    var uri = new Uri("https://" + TitleId + "." + OriginalUrl + req.RawUrl);
                    // Check url starts with Client/
                    if (req.Url?.AbsolutePath.StartsWith("/Client/") ?? true)
                    {
                        uri = new Uri("https://www." + OriginalUrl + req.RawUrl);
                    }

                    client.DefaultRequestHeaders.Add("Host", uri.Host);

                    var request = new HttpRequestMessage(new HttpMethod(req.HttpMethod), uri);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, req.Headers["Content-Type"]);

                    var response = await client.SendAsync(request);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    Log.Logger.Debug("Response: {StatusCode}", response.StatusCode);

                    foreach (var header in response.Headers)
                    {
                        Log.Logger.Debug("Response: {Header} {Value}", header.Key, header.Value);
                    }

                    Log.Logger.Debug("Response: {Body}", responseContent);

                    resp.StatusCode = (int)response.StatusCode;
                    foreach (var header in response.Headers)
                    {
                        foreach (var value in header.Value)
                        {
                            resp.Headers.Add(header.Key, value);
                        }
                    }

                    var buffer = Encoding.UTF8.GetBytes(responseContent);
                    resp.ContentLength64 = buffer.Length;


                    // Write packets to file using url
                    await using (var file = new StreamWriter(
                                     PacketDir + "/" + req.Url?.AbsolutePath.Replace("/", "_") + ".txt", false))
                    {
                        await file.WriteLineAsync(requestBody);
                    }

                    await using (var file = new StreamWriter(
                                     PacketDir + "/" + req.Url?.AbsolutePath.Replace("/", "_") + "_resp.txt", false))
                    {
                        await file.WriteLineAsync(responseContent);
                    }

                    await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
        }
    }
}