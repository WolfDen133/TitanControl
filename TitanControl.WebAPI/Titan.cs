using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
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

            _queue = new PriorityTaskQueue();
            _http = new HttpClient() 
            { 
                BaseAddress = new Uri($"{protocol}://{consoleAddress}:{port}")
            };

            _httpHandler = CreateHandler(localInterfaceAddress);

            Handles = new Handles(_http, _queue);
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

        public Device? ConnectedDevice { get; set; }

        public Handles Handles { get; private set; }

        public Task<bool> IsConnected()
        {
            return _queue.Enqueue(async token =>
            {
                try
                {

                    var response = await _http.GetAsync("titan/get/2/Titan/DeviceInfo");

                    if (response.IsSuccessStatusCode)
                    {
                        ConnectedDevice = await response.Content.ReadFromJsonAsync<Device>();

                        return true;
                    }

                    return false;
                } catch (HttpRequestException e) 
                {
                    Log.Debug($"Failed to locate endpoint after timing out.");
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
