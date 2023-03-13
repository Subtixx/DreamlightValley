using System.Net;
using System.Text;
using Serilog;
using System;

namespace PlayfabApi
{
    public class HttpServer
    {
        private readonly HttpListener? _listener;

        private readonly string _originalUrl;
        private readonly string _titleId;
        private readonly string _packetDir;

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

                var requestBody = await new StreamReader(req.InputStream).ReadToEndAsync();
                
                var responseFromServer = await RelayPacket(req, requestBody);
                await resp.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(responseFromServer));
                resp.Close();
            }
        }

        /// <summary>
        /// Relays the packet to the original server and returns the response
        /// </summary>
        /// <param name="req">Original request</param>
        /// <param name="requestBody">Original request body</param>
        /// <returns>Response from the original server</returns>
        private async Task<string> RelayPacket(HttpListenerRequest req, string requestBody)
        {
            using var client = new HttpClient();

            foreach (var header in req.Headers.AllKeys)
            {
                if (header is null or "Host" or "Content-Length" or "Content-Type")
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

            await DumpPacket(req, requestBody, responseContent);

            return responseContent;
        }

        /// <summary>
        /// Dumps the req packet and response to file
        /// </summary>
        /// <param name="req">Original request</param>
        /// <param name="requestBody">Original request body</param>
        /// <param name="responseContent">Response from the original server</param>
        private async Task DumpPacket(HttpListenerRequest req, string requestBody, string responseContent)
        {
            var fileExtension = req.Headers["Content-Type"] switch
            {
                "application/json" => "json",
                "application/xml" => "xml",
                _ => "txt"
            };
            // Write packets to file using url
            await using (var file = new StreamWriter(
                             _packetDir + "/" + req.Url?.AbsolutePath.Replace("/", "_") + "." + fileExtension, false))
            {
                await file.WriteLineAsync(requestBody);
            }

            await using (var file = new StreamWriter(
                             _packetDir + "/" + req.Url?.AbsolutePath.Replace("/", "_") + "_resp." + fileExtension, false))
            {
                await file.WriteLineAsync(responseContent);
            }
        }
    }
}