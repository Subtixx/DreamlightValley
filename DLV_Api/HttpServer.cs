using System.Net;
using System.Reflection;
using System.Text;
using Serilog;

namespace DLV_Api
{
    public class HttpServer
    {
        private readonly HttpListener? _listener;

        private readonly string _originalUrl;
        private readonly string _titleId;
        private readonly string _packetDir;

        public delegate string EndpointMethod(HttpListenerRequest request, string requestBody, string responseBody);

        private readonly List<EndpointMethod> _endpointMethods;

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
            var listenPrefix = $"http://{listenIpString}:{listenPort}/";
            _listener.Prefixes.Add(listenPrefix);
            Log.Information("HTTP Server will listen on {Prefix}", listenPrefix);

            if (!Directory.Exists(_packetDir))
            {
                Directory.CreateDirectory(_packetDir);
            }
            
            var methods = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(x => x.GetMethods())
                .Where(x => x.GetCustomAttributes(typeof(EndpointAttribute), false).FirstOrDefault() != null);


            _endpointMethods = new List<EndpointMethod>();
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length != 3)
                {
                    Log.Error("Endpoint method {Method} has an invalid number of parameters", method.Name);
                    continue;
                }

                if (parameters[0].ParameterType != typeof(HttpListenerRequest))
                {
                    Log.Error("Endpoint method {Method} has an invalid parameter type for parameter 0", method.Name);
                    continue;
                }

                if (parameters[1].ParameterType != typeof(string))
                {
                    Log.Error("Endpoint method {Method} has an invalid parameter type for parameter 1", method.Name);
                    continue;
                }

                if (parameters[2].ParameterType != typeof(string))
                {
                    Log.Error("Endpoint method {Method} has an invalid parameter type for parameter 2", method.Name);
                    continue;
                }

                if (method.ReturnType != typeof(string))
                {
                    Log.Error("Endpoint method {Method} has an invalid return type", method.Name);
                    continue;
                }

                _endpointMethods.Add((EndpointMethod)Delegate.CreateDelegate(typeof(EndpointMethod), method));
            }

            Log.Information("Loaded {Count} endpoint methods", _endpointMethods.Count);
        }

        public EndpointMethod? GetEndpointMethod(HttpListenerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Url == null)
            {
                throw new ArgumentNullException(nameof(request.Url));
            }

            var endpoint = request.Url.AbsolutePath;
            var method = EndpointAttribute.GetHttpMethod(request.HttpMethod);

            var endpointMethod = _endpointMethods.FirstOrDefault(x =>
                x.Method.GetCustomAttributes(typeof(EndpointAttribute), false).FirstOrDefault() is EndpointAttribute
                    attribute && attribute.Path == endpoint && attribute.Method == method);
            if (endpointMethod != null)
            {
                return endpointMethod;
            }

            Log.Error("No endpoint method found for {Endpoint} {Method}", endpoint, method);
            return null;
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
                // Peel out the requests and response objects
                var req = ctx.Request;
                var resp = ctx.Response;
                try
                {
                    var requestBody = await new StreamReader(req.InputStream).ReadToEndAsync();

                    var responseFromServer = await RelayPacket(req, requestBody);

                    Log.Information("Received request from {Client} to {Url}", req.RemoteEndPoint, req.Url);

                    var origServerResponse = responseFromServer.Body;

                    var endPointMethod = GetEndpointMethod(req);
                    var response = endPointMethod?.Invoke(req, requestBody, origServerResponse);
                    if (response != null)
                    {
                        origServerResponse = response;
                    }

                    resp.StatusCode = responseFromServer.StatusCode;
                    resp.ContentType = responseFromServer.ContentType;

                    var buffer = Encoding.UTF8.GetBytes(origServerResponse);
                    resp.ContentLength64 = buffer.Length;
                    await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);

                    Log.Information("Sent response to {Client}", req.RemoteEndPoint);
                }
                catch (Exception e)
                {
                    Log.Fatal(e, "Error while handling incoming connections");
                }
                finally
                {
                    resp.Close();
                }
            }
        }

        /// <summary>
        /// Relays the packet to the original server and returns the response
        /// </summary>
        /// <param name="req">Original request</param>
        /// <param name="requestBody">Original request body</param>
        /// <returns>Response from the original server</returns>
        private async Task<HttpResponse> RelayPacket(HttpListenerRequest req, string requestBody)
        {
            using var client = new HttpClient();

            foreach (var header in req.Headers.AllKeys)
            {
                if (header is null or "Host" or "Content-Length" or "Content-Type" or "Transfer-Encoding")
                {
                    continue;
                }

                client.DefaultRequestHeaders.Add(header, req.Headers[header]);
            }

            var uri = new Uri("https://" + _titleId + "." + _originalUrl + req.RawUrl);
            // Check url starts with Client/
            if (req.Url?.AbsolutePath.StartsWith("/Client/") ?? true)
            {
                uri = new Uri("https://www." + _originalUrl + req.RawUrl);
            }

            client.DefaultRequestHeaders.Add("Host", uri.Host);

            var request = new HttpRequestMessage(new HttpMethod(req.HttpMethod), uri);
            request.Content = new StringContent(requestBody, Encoding.UTF8, req.Headers["Content-Type"]);

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(_packetDir))
            {
                await DumpPacket(req, response, requestBody, responseContent);
            }

            Log.Debug("Returning response from {Url}", uri.AbsolutePath);

            return new HttpResponse(response.StatusCode, response.Headers, responseContent);
        }

        /// <summary>
        /// Dumps the req packet and response to file
        /// </summary>
        /// <param name="req">Original request</param>
        /// <param name="resp">Response from the original server</param>
        /// <param name="requestBody">Original request body</param>
        /// <param name="responseContent">Response from the original server</param>
        private async Task DumpPacket(HttpListenerRequest req, HttpResponseMessage resp, string requestBody,
            string responseContent)
        {
            Log.Debug("{Method} Request to {Url}", req.HttpMethod, req.Url?.AbsolutePath);
            Log.Debug("Request Headers: {@Headers}", req.Headers);
            Log.Debug("{RequestBody}", requestBody);

            Log.Debug("--------------------");

            Log.Debug("{StatusCode} Response from {Url}", resp.StatusCode, req.Url?.AbsolutePath);
            Log.Debug("Response Headers: {Headers}", resp.Headers);
            Log.Debug("{ResponseContent}", responseContent);

            var fileExtension = req.Headers["Content-Type"] switch
            {
                "application/json" => "json",
                "application/xml" => "xml",
                _ => "txt"
            };
            
            var requestPath = _packetDir + "/" + req.Url?.AbsolutePath.Replace("/", "_") + "." + fileExtension;
            if(!File.Exists(requestPath))
            {
                await using var file = new StreamWriter(requestPath, false);
                await file.WriteLineAsync(requestBody);
            }

            var responsePath = _packetDir + "/" + req.Url?.AbsolutePath.Replace("/", "_") + "_resp." + fileExtension;
            if (!File.Exists(responsePath))
            {
                await using var file = new StreamWriter(responsePath, false);
                await file.WriteLineAsync(responseContent);
            }
        }
    }
}