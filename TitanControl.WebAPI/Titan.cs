using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.WebAPI.Data.Model;
using TitanControl.WebAPI.Queue;
using static System.Net.WebRequestMethods;

namespace TitanControl.WebAPI
{
    public class Titan : IDisposable
    {
        public const int NormalPort = 4430;
        public const int InteractivePort = 4431;

        private readonly PriorityTaskQueue _queue;
        private readonly HttpClient _http;
        private readonly SocketsHttpHandler _httpHandler;

        public IPAddress Address => IPAddress.Parse(_http.BaseAddress?.Host ?? "0.0.0.0");
        public int Port => _http.BaseAddress?.Port ?? -1;
        public int PortInteractive => _http.BaseAddress?.Port ?? -1;

        public Titan(IPAddress consoleAddress) : this(consoleAddress, NormalPort)
        {
        }

        public Titan(IPAddress consoleAddress, int port) : this(consoleAddress, port, -1)
        {

        }

        public Titan(string consoleAddress) : this(IPAddress.Parse(consoleAddress), NormalPort)
        {

        }

        public Titan(IPAddress consoleAddress, int port, int interactivePort = -1, bool https = false, IPAddress? localInterfaceAddress = null)
        {
            ArgumentNullException.ThrowIfNull(consoleAddress);
            if (localInterfaceAddress is null) localInterfaceAddress = new IPAddress([127, 0, 0, 1]);

            if (consoleAddress.AddressFamily != localInterfaceAddress.AddressFamily)
            {
                throw new ArgumentException(
                    "The console address and local interface address " +
                    "must use the same address family.");
            }

            string protocol = https ? "https" : "http";

            _httpHandler = CreateHandler(localInterfaceAddress);
            _http = new HttpClient(_httpHandler) 
            { 
                BaseAddress = new Uri($"{protocol}://{consoleAddress}:{port}"),
            };

            _queue = new PriorityTaskQueue();
            CueLists = new CueLists(_http, _queue);
            Dmx = new Dmx(_http, _queue);
            Fixtures = new Fixtures(_http, _queue); 
            Groups = new Groups(_http, _queue);
            Handles = new Handles(_http, _queue);
            Macros = new Macros(_http, _queue);
            Masters = new Masters(_http, _queue);
            Menu = new Menu(_http, _queue);
            Palettes = new Palettes(_http, _queue);
            Playbacks = new Playbacks(_http, _queue);
            Programmer = new Programmer(_http, _queue);
            SelectIf = new SelectIf(_http, _queue);
            Selection = new Selection(_http, _queue);
            SetList = new SetList(_http, _queue);
        }

        private static SocketsHttpHandler CreateHandler(IPAddress localInterfaceAddress)
        {
            return new SocketsHttpHandler
            {
                ConnectCallback = async (
                    context,
                    cancellationToken) =>
                {
                    var socket = new Socket(
                        localInterfaceAddress.AddressFamily,
                        SocketType.Stream,
                        ProtocolType.Tcp);

                    try
                    {
                        // Port 0 means Windows chooses an available
                        // ephemeral source port.
                        socket.Bind(
                            new IPEndPoint(
                                localInterfaceAddress,
                                0));

                        await socket.ConnectAsync(
                            context.DnsEndPoint,
                            cancellationToken);

                        return new NetworkStream(
                            socket,
                            ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }
            };
        }

        public void Start()
        {
            _queue.Start();
        }

        public void Stop()
        {
            _queue.Stop();
        }

        public Task StopAsync()
        {
            return _queue.StopAsync();
        }

        public Device? ConnectedDevice { get; set; }

        public CueLists CueLists { get; private set; }
        public Dmx Dmx { get; private set; }
        public Fixtures Fixtures { get; private set; }
        public Groups Groups { get; private set; }
        public Handles Handles { get; private set; }
        public Macros Macros { get; private set; }
        public Masters Masters { get; private set; }
        public Menu Menu { get; private set; }
        public Palettes Palettes { get; private set; }
        public Playbacks Playbacks { get; private set; }
        public Programmer Programmer { get; private set; }
        public SelectIf SelectIf { get; private set; }
        public Selection Selection { get; private set; }
        public SetList SetList { get; private set; }

        public Task<bool> IsConnected()
        {
            return _queue.Enqueue(async token =>
            {
                try
                {

                    var response = await _http.GetAsync("titan/get/2/Titan/DeviceInfo", token);

                    if (response.IsSuccessStatusCode)
                    {
                        ConnectedDevice = await response.Content.ReadFromJsonAsync<Device>();

                        return true;
                    }

                    return false;
                } catch (HttpRequestException) 
                {
                    return false;
                }
            }, priority: TaskPriority.High);
        }

        public void Dispose()
        {
            _http.Dispose();
            _queue.Dispose();
        }
    }
}
