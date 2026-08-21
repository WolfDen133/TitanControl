using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TitanControl.Logging;
using TitanControl.WebAPI.Data.Model;

namespace TitanControl.Services.Session
{
    /// <summary>
    /// Continuously scans the subnet associated with a selected local IPv4
    /// interface for Titan WebAPI endpoints.
    ///
    /// Existing sessions supplied to StartAsync are validated in place.
    /// Newly discovered sessions are exposed through Results.
    /// </summary>
    public sealed class SessionScanner : IAsyncDisposable
    {
        public const int NormalPort = 4430;
        public const int InteractivePort = 4431;

        private readonly IPAddress _localInterfaceAddress;
        private readonly IPAddress _subnetMask;
        private readonly TimeSpan _scanDuration;
        private readonly TimeSpan _connectionTimeout;
        private readonly TimeSpan _delayBetweenScans;
        private readonly int _maximumConcurrency;
        private readonly bool _useHttps;
        private readonly HttpClient _http;

        private readonly ObservableCollection<ISession> _results = new();
        private readonly ReadOnlyObservableCollection<ISession> _readOnlyResults;

        // Contains only sessions discovered by this scanner. Existing persisted
        // sessions supplied to StartAsync are kept separately and are updated
        // directly rather than copied into Results.
        private readonly ConcurrentDictionary<string, TitanSession> _knownSessions =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly SynchronizationContext? _synchronizationContext;
        private readonly SemaphoreSlim _runLock = new(1, 1);

        private IReadOnlyList<TitanSession> _existingSessions =
            Array.Empty<TitanSession>();

        private CancellationTokenSource? _scannerCancellation;
        private Task? _scannerTask;
        private bool _disposed;

        /// <summary>
        /// Gets newly discovered Titan sessions that are not already represented
        /// by one of the existing sessions supplied to StartAsync.
        /// </summary>
        public ReadOnlyObservableCollection<ISession> Results =>
            _readOnlyResults;

        /// <summary>
        /// Gets whether this scanner instance is currently running.
        /// </summary>
        public bool IsRunning =>
            _scannerTask is { IsCompleted: false };

        /// <summary>
        /// Gets the IPv4 address of the network interface used by this scanner.
        /// </summary>
        public IPAddress LocalInterfaceAddress =>
            _localInterfaceAddress;

        /// <summary>
        /// Raised when the continuous scanner starts or stops.
        /// </summary>
        public event EventHandler<bool>? OnScannerRunning;

        public event NotifyCollectionChangedEventHandler? ResultsChanged
        {
            add => ((INotifyCollectionChanged)_readOnlyResults).CollectionChanged += value;
            remove => ((INotifyCollectionChanged)_readOnlyResults).CollectionChanged -= value;
        }

        /// <summary>
        /// Creates a scanner for the subnet associated with the supplied local
        /// IPv4 interface address.
        /// </summary>
        /// <param name="localInterfaceAddress">
        /// An IPv4 address assigned to the NIC that should be used.
        /// </param>
        /// <param name="scanDuration">
        /// Maximum amount of time the scanner should run.
        /// </param>
        /// <param name="connectionTimeout">
        /// Maximum time allowed for each individual connection or DeviceInfo request.
        /// </param>
        /// <param name="delayBetweenScans">
        /// Delay between completed subnet scan passes.
        /// </param>
        /// <param name="maximumConcurrency">
        /// Maximum number of simultaneous connection attempts.
        /// </param>
        /// <param name="useHttps">
        /// Protocol used when discovering new sessions on port 4430.
        /// Existing sessions are validated using their own UseHttps value.
        /// </param>
        public SessionScanner(
            IPAddress localInterfaceAddress,
            TimeSpan scanDuration,
            TimeSpan? connectionTimeout = null,
            TimeSpan? delayBetweenScans = null,
            int maximumConcurrency = 64,
            bool useHttps = false)
        {
            ArgumentNullException.ThrowIfNull(localInterfaceAddress);

            if (localInterfaceAddress.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException(
                    "Only IPv4 network interfaces are currently supported.",
                    nameof(localInterfaceAddress));
            }

            if (scanDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(scanDuration),
                    "The scan duration must be greater than zero.");
            }

            if (maximumConcurrency <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumConcurrency),
                    "Maximum concurrency must be greater than zero.");
            }

            _connectionTimeout =
                connectionTimeout ?? TimeSpan.FromMilliseconds(500);

            if (_connectionTimeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(connectionTimeout),
                    "The connection timeout must be greater than zero.");
            }

            _delayBetweenScans =
                delayBetweenScans ?? TimeSpan.FromSeconds(1);

            if (_delayBetweenScans < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(delayBetweenScans),
                    "The delay between scans cannot be negative.");
            }

            _localInterfaceAddress = localInterfaceAddress;
            _subnetMask = FindSubnetMask(localInterfaceAddress);
            _scanDuration = scanDuration;
            _maximumConcurrency = maximumConcurrency;
            _useHttps = useHttps;
            _http = CreateHttpClient(localInterfaceAddress);

            // Construct the scanner on the UI thread if Results and TitanSession
            // properties are bound directly to UI controls.
            _synchronizationContext = SynchronizationContext.Current;

            _readOnlyResults =
                new ReadOnlyObservableCollection<ISession>(_results);

            Log.Debug(
                $"Created session scanner on {_localInterfaceAddress}.",
                "SessionScanner",
                new Dictionary<string, object?>
                {
                    ["SubnetMask"] = _subnetMask,
                    ["ScanDuration"] = _scanDuration,
                    ["ConnectionTimeout"] = _connectionTimeout,
                    ["MaximumConcurrency"] = _maximumConcurrency,
                    ["DiscoveryHttps"] = _useHttps
                });
        }

        /// <summary>
        /// Starts scanning without a pre-existing session list.
        /// </summary>
        public Task StartAsync(
            CancellationToken cancellationToken = default)
        {
            return StartAsync(
                new(),
                cancellationToken);
        }

        /// <summary>
        /// Starts continuous discovery and validates the supplied existing
        /// sessions in place on every scan pass.
        /// </summary>
        /// <remarks>
        /// The supplied collection is snapshotted when scanning starts, but the
        /// TitanSession objects themselves are not copied. State, ComputerName and
        /// PortInteractive changes are therefore applied to the caller's objects.
        /// </remarks>
        public async Task StartAsync(
            ObservableCollection<TitanSession> existingSessions,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(existingSessions);

            await _runLock
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                if (_scannerTask is { IsCompleted: false })
                {
                    Log.Warning(
                        "Attempted to start an already running scanner.",
                        "SessionScanner");

                    throw new InvalidOperationException(
                        "This network scanner is already running.");
                }

                // Preserve object references so PropertyChanged is raised on the
                // exact TitanSession instances owned by the caller.
                _existingSessions = existingSessions
                    .Where(session => session is not null)
                    .ToArray();

                _scannerCancellation?.Dispose();
                _scannerCancellation =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken);

                _scannerCancellation.CancelAfter(_scanDuration);

                Log.Information(
                    "Starting automatic session discovery.",
                    "SessionScanner",
                    new Dictionary<string, object?>
                    {
                        ["Interface"] = _localInterfaceAddress,
                        ["ScanDuration"] = _scanDuration,
                        ["Concurrency"] = _maximumConcurrency,
                        ["ExistingSessions"] = _existingSessions.Count
                    });

                _scannerTask = ScanLoopAsync(
                    _scannerCancellation.Token);
            }
            finally
            {
                _runLock.Release();
            }

            try
            {
                await _scannerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
                when (_scannerCancellation?.IsCancellationRequested == true)
            {
                Log.Information(
                    "Automatic session discovery stopped.",
                    "SessionScanner");
            }
        }

        /// <summary>
        /// Requests that the current continuous scan stop.
        /// </summary>
        public void Stop()
        {
            ThrowIfDisposed();

            _scannerCancellation?.Cancel();

            Log.Information(
                "Stopping session discovery.",
                "SessionScanner");
        }

        /// <summary>
        /// Removes all newly discovered sessions from Results. This does not
        /// modify the existing sessions that were supplied to StartAsync.
        /// </summary>
        public async Task ClearResultsAsync()
        {
            ThrowIfDisposed();

            _knownSessions.Clear();

            await RunOnCapturedContextAsync(
                    _results.Clear)
                .ConfigureAwait(false);
        }

        private async Task ScanLoopAsync(
            CancellationToken cancellationToken)
        {
            await SetScannerRunningAsync(true)
                .ConfigureAwait(false);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Log.Debug(
                        "Beginning subnet scan.",
                        "SessionScanner");

                    await ScanSubnetOnceAsync(cancellationToken)
                        .ConfigureAwait(false);

                    Log.Debug(
                        $"Subnet scan complete. {_results.Count} new sessions discovered.",
                        "SessionScanner");

                    if (_delayBetweenScans == TimeSpan.Zero)
                        continue;

                    await Task.Delay(
                            _delayBetweenScans,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            finally
            {
                await SetScannerRunningAsync(false)
                    .ConfigureAwait(false);
            }
        }

        private async Task ScanSubnetOnceAsync(
            CancellationToken cancellationToken)
        {
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _maximumConcurrency
            };

            await Parallel.ForEachAsync(
                    EnumerateSubnetAddresses(
                        _localInterfaceAddress,
                        _subnetMask),
                    options,
                    async (address, token) =>
                    {
                        await ScanAddressAsync(address, token)
                            .ConfigureAwait(false);
                    })
                .ConfigureAwait(false);
        }

        private async Task ScanAddressAsync(
            IPAddress address,
            CancellationToken cancellationToken)
        {
            string key = CreateDiscoveryKey(
                address,
                NormalPort,
                _useHttps);

            // Previously discovered sessions are updated in place. TitanSession
            // raises PropertyChanged, so the ObservableCollection item does not
            // need to be removed/reinserted.
            if (_knownSessions.TryGetValue(
                    key,
                    out TitanSession? knownSession))
            {
                await RefreshDiscoveredSessionAsync(
                        knownSession,
                        cancellationToken)
                    .ConfigureAwait(false);

                return;
            }

            bool normalPortAvailable = await IsPortOpenAsync(
                    address,
                    NormalPort,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!normalPortAvailable)
                return;

            Device? device = await GetDeviceInfoAsync(
                    address,
                    NormalPort,
                    _useHttps,
                    cancellationToken)
                .ConfigureAwait(false);

            // An open port alone is not enough to identify a Titan endpoint.
            if (device is null)
                return;

            int? interactivePort = await DetectInteractivePortAsync(
                    address,
                    cancellationToken)
                .ConfigureAwait(false);

            // Do not report a persisted session as a new discovery. The internal
            // Guid is intentionally ignored; endpoint settings + DeviceInfo are
            // used to establish identity.
            ISession? existingSession = FindMatchingExistingSession(
                address,
                NormalPort,
                _useHttps,
                device);

            if (existingSession is not null)
            {
                await RunOnCapturedContextAsync(() =>
                {
                    existingSession.ComputerName = device.ComputerName ?? string.Empty;
                    existingSession.PortInteractive = interactivePort;
                    existingSession.State = SessionConnectionState.Disabled;
                }).ConfigureAwait(false);

                Log.Trace(
                    $"Discovered endpoint {address} matches an existing session.",
                    "SessionScanner",
                    new Dictionary<string, object?>
                    {
                        ["Legend"] = device.Legend,
                        ["ComputerName"] = device.ComputerName,
                        ["NormalPort"] = NormalPort,
                        ["Https"] = _useHttps
                    });

                return;
            }

            var session = new TitanSession(Guid.NewGuid(), device.Legend ?? string.Empty)
            {
                ComputerName = device.ComputerName ?? string.Empty,
                IPAddress = address,
                Port = NormalPort,
                PortInteractive = interactivePort,
                UseHttps = _useHttps,
                State = SessionConnectionState.Available,
                IsSelected = false
            };

            if (!_knownSessions.TryAdd(key, session))
                return;

            Log.Information(
                $"Discovered new Titan session {address}.",
                "SessionScanner",
                new Dictionary<string, object?>
                {
                    ["IPAddress"] = address,
                    ["ComputerName"] = device.ComputerName,
                    ["Legend"] = device.Legend,
                    ["NormalPort"] = NormalPort,
                    ["InteractivePort"] = interactivePort,
                    ["Https"] = _useHttps
                });

            await RunOnCapturedContextAsync(() =>
            {
                _results.Add(session);
            }).ConfigureAwait(false);
        }

        private async Task<bool> RefreshDiscoveredSessionAsync(
            ISession session,
            CancellationToken cancellationToken)
        {
            Device? device = await GetDeviceInfoAsync(
                    session.IPAddress,
                    session.Port,
                    session.UseHttps,
                    cancellationToken)
                .ConfigureAwait(false);

            if (device is null)
            {
                await RunOnCapturedContextAsync(() =>
                {
                    session.PortInteractive = null;
                    session.State = SessionConnectionState.Disabled;
                }).ConfigureAwait(false);

                return false;
            }

            int? interactivePort = await DetectInteractivePortAsync(
                    session.IPAddress,
                    cancellationToken)
                .ConfigureAwait(false);

            await RunOnCapturedContextAsync(() =>
            {
                session.Name = device.Legend ?? string.Empty;
                session.ComputerName = device.ComputerName ?? string.Empty;
                session.PortInteractive = interactivePort;
                session.State = SessionConnectionState.Available;
            }).ConfigureAwait(false);

            return true;
        }

        private TitanSession? FindMatchingExistingSession(
            IPAddress address,
            int port,
            bool useHttps,
            Device device)
        {
            foreach (TitanSession session in _existingSessions)
            {
                if (!session.IPAddress.Equals(address))
                    continue;

                if (session.Port != port)
                    continue;

                if (session.UseHttps != useHttps)
                    continue;

                if (!IdentityEquals(session.Name, device.Legend))
                    continue;

                // See ValidateExistingSessionAsync: blank ComputerName means it
                // has not yet been learned from DeviceInfo.
                if (!string.IsNullOrWhiteSpace(session.ComputerName) &&
                    !IdentityEquals(session.ComputerName, device.ComputerName))
                {
                    continue;
                }

                return session;
            }

            return null;
        }

        private static bool IdentityEquals(
            string? left,
            string? right)
        {
            return string.Equals(
                left?.Trim(),
                right?.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        private async Task<int?> DetectInteractivePortAsync(
            IPAddress address,
            CancellationToken cancellationToken)
        {
            bool available = await IsPortOpenAsync(
                    address,
                    InteractivePort,
                    cancellationToken)
                .ConfigureAwait(false);

            return available
                ? InteractivePort
                : null;
        }

        private async Task<Device?> GetDeviceInfoAsync(
            IPAddress address,
            int port,
            bool useHttps,
            CancellationToken cancellationToken)
        {
            using var timeoutCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            timeoutCancellation.CancelAfter(_connectionTimeout);

            string protocol = useHttps ? "https" : "http";
            var requestUri = new Uri(
                $"{protocol}://{address}:{port}/" +
                "titan/get/2/Titan/DeviceInfo");

            try
            {
                using HttpResponseMessage response =
                    await _http.GetAsync(
                            requestUri,
                            HttpCompletionOption.ResponseHeadersRead,
                            timeoutCancellation.Token)
                        .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Trace(
                        $"DeviceInfo returned HTTP {(int)response.StatusCode} for {address}:{port}.",
                        "SessionScanner",
                        new Dictionary<string, object?>
                        {
                            ["Https"] = useHttps,
                            ["StatusCode"] = response.StatusCode
                        });

                    return null;
                }

                return await response.Content
                    .ReadFromJsonAsync<Device>(
                        cancellationToken: timeoutCancellation.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                Log.Trace(
                    $"Device information request timed out for {address}:{port}.",
                    "SessionScanner",
                    new Dictionary<string, object?>
                    {
                        ["Https"] = useHttps
                    });

                return null;
            }
            catch (HttpRequestException exception)
            {
                Log.Trace(
                    $"Device information request failed for {address}:{port}.",
                    "SessionScanner",
                    new Dictionary<string, object?>
                    {
                        ["Https"] = useHttps,
                        ["Error"] = exception.Message
                    });

                return null;
            }
            catch (JsonException exception)
            {
                Log.Warning(
                    $"Device information from {address}:{port} was invalid.",
                    "SessionScanner",
                    new Dictionary<string, object?>
                    {
                        ["Https"] = useHttps,
                        ["Error"] = exception.Message
                    });

                return null;
            }
        }

        private static HttpClient CreateHttpClient(
            IPAddress localInterfaceAddress)
        {
            var handler = new SocketsHttpHandler
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
                        socket.Bind(
                            new IPEndPoint(localInterfaceAddress, 0));

                        await socket.ConnectAsync(
                                context.DnsEndPoint,
                                cancellationToken)
                            .ConfigureAwait(false);

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

            return new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        private async Task<bool> IsPortOpenAsync(
            IPAddress remoteAddress,
            int port,
            CancellationToken cancellationToken)
        {
            using var timeoutCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            timeoutCancellation.CancelAfter(_connectionTimeout);

            using var client = new TcpClient(
                AddressFamily.InterNetwork);

            try
            {
                // Bind the connection to the selected network interface.
                client.Client.Bind(
                    new IPEndPoint(
                        _localInterfaceAddress,
                        0));

                await client.ConnectAsync(
                        remoteAddress,
                        port,
                        timeoutCancellation.Token)
                    .ConfigureAwait(false);

                return true;
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            catch (SocketException exception)
            {
                Log.Trace(
                    $"Port {port} closed on {remoteAddress}.",
                    "SessionScanner",
                    new Dictionary<string, object?>
                    {
                        ["SocketError"] = exception.SocketErrorCode
                    });

                return false;
            }
        }

        private Task SetScannerRunningAsync(bool running)
        {
            return RunOnCapturedContextAsync(() =>
            {
                OnScannerRunning?.Invoke(this, running);
            });
        }

        private Task RunOnCapturedContextAsync(Action action)
        {
            if (_synchronizationContext is null ||
                SynchronizationContext.Current == _synchronizationContext)
            {
                action();
                return Task.CompletedTask;
            }

            var completion =
                new TaskCompletionSource<object?>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            _synchronizationContext.Post(
                _ =>
                {
                    try
                    {
                        action();
                        completion.SetResult(null);
                    }
                    catch (Exception exception)
                    {
                        Log.Error(
                            exception,
                            "UI synchronization failed.",
                            "SessionScanner");

                        completion.SetException(exception);
                    }
                },
                null);

            return completion.Task;
        }

        private static IPAddress FindSubnetMask(
            IPAddress localAddress)
        {
            foreach (NetworkInterface networkInterface in
                     NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastAddress in
                         networkInterface
                             .GetIPProperties()
                             .UnicastAddresses)
                {
                    if (!unicastAddress.Address.Equals(localAddress))
                        continue;

                    if (unicastAddress.IPv4Mask is null)
                    {
                        throw new InvalidOperationException(
                            $"No IPv4 subnet mask was found for " +
                            $"{localAddress}.");
                    }

                    return unicastAddress.IPv4Mask;
                }
            }

            throw new ArgumentException(
                $"The address {localAddress} is not assigned to an " +
                "available network interface.",
                nameof(localAddress));
        }

        private static IEnumerable<IPAddress> EnumerateSubnetAddresses(
            IPAddress localAddress,
            IPAddress subnetMask)
        {
            // The entire 127.0.0.0/8 range is loopback. Only probe the selected
            // loopback address once rather than treating every 127.x.x.x address
            // as a separate device.
            if (IPAddress.IsLoopback(localAddress))
            {
                yield return localAddress;
                yield break;
            }

            uint local = ToUInt32(localAddress);
            uint mask = ToUInt32(subnetMask);

            uint network = local & mask;
            uint broadcast = network | ~mask;

            if (broadcast - network <= 1)
            {
                yield return localAddress;
                yield break;
            }

            for (uint current = network + 1;
                 current < broadcast;
                 current++)
            {
                yield return FromUInt32(current);
            }
        }

        private static uint ToUInt32(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();

            return
                ((uint)bytes[0] << 24) |
                ((uint)bytes[1] << 16) |
                ((uint)bytes[2] << 8) |
                bytes[3];
        }

        private static IPAddress FromUInt32(uint address)
        {
            return new IPAddress(
            [
                (byte)(address >> 24),
                (byte)(address >> 16),
                (byte)(address >> 8),
                (byte)address
            ]);
        }

        private static string CreateDiscoveryKey(
            IPAddress address,
            int port,
            bool useHttps)
        {
            return $"{address}|{port}|{useHttps}";
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(
                _disposed,
                this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            Log.Information(
                "Disposing session scanner.",
                "SessionScanner");

            _disposed = true;
            _scannerCancellation?.Cancel();

            if (_scannerTask is not null)
            {
                try
                {
                    await _scannerTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Expected when disposal cancels an active scan.
                }
            }

            _scannerCancellation?.Dispose();
            _http.Dispose();
            _runLock.Dispose();

            await RunOnCapturedContextAsync(
                    _results.Clear)
                .ConfigureAwait(false);

            _knownSessions.Clear();
            _existingSessions = Array.Empty<TitanSession>();
        }
    }
}