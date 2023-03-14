using System.Net;
using System.Reflection;
using System.Text;
using Serilog;

namespace DLV_Api.Http
{
    public class HttpServer
    {
        private readonly HttpListener? _listener;

        private readonly string _originalUrl;
        private readonly string _titleId;
        private readonly string _packetDir;


        private readonly Dictionary<string, ApiRequest> _apiRequests = new();

        /// <summary>
        /// Create a new HTTP server
        /// </summary>
        /// <param name="listenIp">IP address to listen on</param>
        /// <param name="listenPort">Port to listen on</param>
        /// <param name="titleId">The title ID of the playfab SDK game</param>
        /// <param name="productionEnvironmentUrl">The original production environment URL to send requests to</param>
        /// <param name="packetDir">A directory to save packets to</param>
        /// <exception cref="ArgumentException"></exception>
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

            if (listenPort == 0)
            {
                throw new ArgumentException("Listen port cannot be 0");
            }

            if (string.IsNullOrEmpty(titleId))
            {
                throw new ArgumentException("Title ID cannot be null or empty");
            }

            if (string.IsNullOrEmpty(productionEnvironmentUrl))
            {
                throw new ArgumentException("Production environment URL cannot be null or empty");
            }

            // Ignore certificate errors
            ServicePointManager.ServerCertificateValidationCallback = delegate
            {
                return true;
            };

            _titleId = titleId;
            _packetDir = packetDir;
            _originalUrl = productionEnvironmentUrl;

            var listenIpString = listenIp.ToString();

            _listener = new HttpListener();
            var listenPrefix = "http:" + $"//{listenIpString}:{listenPort}/";
            _listener.Prefixes.Add(listenPrefix);
            Log.Information("HTTP Server will listen on {Prefix}", listenPrefix);

            if (!Directory.Exists(_packetDir))
            {
                Directory.CreateDirectory(_packetDir);
            }

            AddPacketHandlers();
        }

        private void AddPacketHandlers()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                // Get classes that inherit from ApiRequest
                if (!type.IsSubclassOf(typeof(ApiRequest)) || type == typeof(ApiRequest))
                {
                    continue;
                }

                var instance = Activator.CreateInstance(type);
                if (instance == null)
                {
                    Log.Error("Failed to create instance of {Type}", type.Name);
                    continue;
                }

                var apiRequest = (ApiRequest)instance;
                if (_apiRequests.ContainsKey(apiRequest.GetPath()))
                {
                    Log.Error("Duplicate packet handler for {Path}", apiRequest.GetPath());
                    continue;
                }

                _apiRequests.Add(apiRequest.GetPath(), apiRequest);
            }

            Log.Information("Added {Count} packet handlers", _apiRequests.Count);
        }

        /// <summary>
        /// Starts the http server
        /// </summary>
        public async Task Start()
        {
            _listener?.Start();

            Log.Information("HTTP Server started");

            // Handle requests
            await HandleIncomingConnections();
        }

        /// <summary>
        /// Stops the http server
        /// </summary>
        public void Stop()
        {
            _listener?.Stop();
        }

        /// <summary>
        /// Main loop that handles incoming connections from http clients
        /// </summary>
        private async Task HandleIncomingConnections()
        {
            while (_listener?.IsListening ?? false)
            {
                // Will wait here until we hear from a connection
                var ctx = await _listener.GetContextAsync();
                OnRequest(ctx);
            }
        }

        private async void OnRequest(HttpListenerContext ctx)
        {
            try
            {
                var req = new HttpRequest(ctx.Request);
                
                var apiRequest = _apiRequests.ContainsKey(req.Url.AbsolutePath)
                    ? _apiRequests[req.Url.AbsolutePath]
                    : null;

                var result = apiRequest?.BeforeRequest(req);
                if (result != null)
                {
                    req.Body = result;
                }

                var responseFromServer = await RelayPacket(req);

                var relayResult = apiRequest?.AfterRequest(responseFromServer);
                if (relayResult != null)
                {
                    responseFromServer.Body = relayResult;
                }

                ctx.Response.StatusCode = (int)responseFromServer.StatusCode;
                ctx.Response.ContentType = responseFromServer.ContentType;
                ctx.Response.ContentEncoding = Encoding.UTF8;
                var bytesWrite = Encoding.UTF8.GetBytes(responseFromServer.Body);
                ctx.Response.ContentLength64 = bytesWrite.Length;
                await ctx.Response.OutputStream.WriteAsync(bytesWrite);

                Log.Debug("Sent response to {Client}", ctx.Request.RemoteEndPoint);
                ctx.Response.OutputStream.Close();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Error while handling incoming connections");
                ctx.Response.OutputStream.Close();
            }
        }

        /// <summary>
        /// Relays the packet to the original server and returns the response
        /// </summary>
        /// <param name="req">Original request</param>
        /// <returns>Response from the original server</returns>
        private async Task<HttpResponse> RelayPacket(HttpRequest req)
        {
            using var client = new HttpClient();

            foreach (var header in req.Headers.Where(header =>
                         header.Key is not ("Host" or "Content-Length" or "Content-Type" or "Transfer-Encoding")))
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            var uri = new Uri("https://" + _titleId + "." + _originalUrl + req.Url.AbsolutePath);
            // Check url starts with Client/
            if (req.Url.AbsolutePath.StartsWith("/Client/") && req.Url.AbsolutePath.Contains("Login"))
            {
                uri = new Uri("https://www." + _originalUrl + req.Url.AbsolutePath);
            }

            req.Url = uri;

            client.DefaultRequestHeaders.Add("Host", uri.Host);

            var request = req.GetHttpRequestMessage();

            var responseMsg = await client.SendAsync(request);

            var response = new HttpResponse(responseMsg);
#if DEBUG
            if (!string.IsNullOrEmpty(_packetDir))
            {
                await DumpPacket(req, response);
            }
#endif

            Log.Debug("Returning response from {Url}", uri.AbsolutePath);

            return response;
        }

        /// <summary>
        /// Dumps the req packet and response to file
        /// </summary>
        /// <param name="req">Original request</param>
        /// <param name="resp">Response from the original server</param>
        private async Task DumpPacket(HttpRequest req, HttpResponse resp)
        {
            Log.Debug("{Method} Request to {Url}", req.Method, req.Url.AbsolutePath);
            Log.Debug("Request Headers: {@Headers}", req.Headers);
            Log.Debug("{RequestBody}", req.Body);

            Log.Debug("--------------------");

            Log.Debug("{StatusCode} Response from {Url}", resp.StatusCode, req.Url.AbsolutePath);
            Log.Debug("Response Headers: {Headers}", resp.Headers);
            Log.Debug("{ResponseContent}", resp.Body);

            var fileExtension = req.Headers["Content-Type"] switch
            {
                "application/json" => "json",
                "application/xml" => "xml",
                _ => "txt"
            };

            // Removes leading slash and file name
            var fi = new FileInfo(Path.Join(_packetDir, req.Url.AbsolutePath));
            if (!fi.Directory?.Exists ?? false)
            {
                fi.Directory?.Create();
            }


            // Unix timestamp
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var requestPath = fi.FullName + "_" + now + "." + fileExtension;
            if (!File.Exists(requestPath))
            {
                await using var file = new StreamWriter(requestPath, false);
                await file.WriteLineAsync(req.Body);
            }

            var responsePath = fi.FullName + "_" + now + ".response." + fileExtension;
            if (!File.Exists(responsePath))
            {
                await using var file = new StreamWriter(responsePath, false);
                await file.WriteLineAsync(resp.Body);
            }
        }
    }
}