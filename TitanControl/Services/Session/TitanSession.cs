using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TitanControl.Events.Session;
using TitanControl.Logging;
using TitanControl.Models;
using TitanControl.Models.Session;
using TitanControl.WebAPI;

namespace TitanControl.Services.Session
{
    public sealed class TitanSession : ISession, ISaveable
    {
        private const string LoggingCategory = "TitanSession";

        private readonly object _sync = new();

        private System.Threading.Timer? _keepAliveTimer;
        private SessionConnectionState _state =
            SessionConnectionState.Disabled;

        private int _consecutiveFailures;
        private int _keepAliveRunning;
        private bool _disposed;
        private bool _isEnabled;

        private string name = string.Empty;
        private IPAddress ipAddress = new IPAddress([127, 0, 0, 1]);
        private int port = 4430;
        private int? portInteractive;
        private bool useHttps = false;
        private int reconnectIterations = 3;
        private int keepAlive = 6;
        private int? autoTimeout = null;
        private string computerName = string.Empty;
        private DateTime? connectedAt;
        private bool isSelected = false;
        private System.Timers.Timer _unreachableReset = new();

        public TitanSession(
            Guid Id,
            string name)
        {
            ArgumentNullException.ThrowIfNull(ID);
            ArgumentNullException.ThrowIfNull(name);

            ID = Id;
            Name = name;

            SetState(SessionConnectionState.Disabled);

            _unreachableReset.Interval = 3000;
            _unreachableReset.Elapsed += UnnreachableReset_Elapsed;
        }

        public Guid ID { get; }
        public Titan? Api { get; private set; }
        public DateTimeOffset? LastSuccessfulKeepAlive { get; private set; }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public bool UseHttps
        {
            get => useHttps;
            set
            {
                useHttps = value;
                OnPropertyChanged(nameof(UseHttps));
            }
        }
        public IPAddress IPAddress
        {
            get => ipAddress;
            set
            {
                ipAddress = value;
                OnPropertyChanged(nameof(IPAddress));
            }
        }
        public int Port
        {
            get => port;
            set
            {
                port = value;
                OnPropertyChanged(nameof(Port));
            }
        }
        public int? PortInteractive
        {
            get => portInteractive;
            set
            {
                portInteractive = value;
                OnPropertyChanged(nameof(PortInteractive));
            }
        }
        public int ReconnectIterations
        {
            get => reconnectIterations;
            set
            {
                reconnectIterations = value;
                OnPropertyChanged(nameof(ReconnectIterations));
            }
        }
        public int KeepAlive
        {
            get => keepAlive;
            set
            {
                keepAlive = value;
                OnPropertyChanged(nameof(KeepAlive));
            }
        }
        public int? AutoTimeout
        {
            get => autoTimeout;
            set
            {
                autoTimeout = value;
                OnPropertyChanged(nameof(AutoTimeout));
            }
        }
        public string ComputerName
        {
            get => computerName;
            set
            {
                computerName = value;
                OnPropertyChanged(nameof(ComputerName));
            }
        }
        public DateTime? ConnectedAt
        {
            get => connectedAt;
            private set
            {
                connectedAt = value;
                OnPropertyChanged(nameof(ConnectedAt));
            }
        }
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public SessionConnectionState State
        {
            get
            {
                lock (_sync)
                {
                    return _state;
                }
            }
            set
            {
                lock (_sync)
                {
                    _state = value;
                }
            }
        }

        private void UnnreachableReset_Elapsed(object? sender, ElapsedEventArgs e)
        {
            _unreachableReset.Stop();

            if (State != SessionConnectionState.Unreachable)
                return;

            if (_isEnabled)
                SetState(SessionConnectionState.Enabled);
            else
                SetState(SessionConnectionState.Disabled);
        }

        public bool IsConnected =>
            State == SessionConnectionState.Connected;

        public event EventHandler<SessionStateChangedEventArgs>? StateChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool wasConnected;

        public async Task Start(IPAddress selectedInterface)
        {
            if (State != SessionConnectionState.Enabled)
                return;

            Log.Debug($"Starting session {Name}({ID}).", LoggingCategory);

            SetState(SessionConnectionState.Connecting);
            ThrowIfDisposed();

            Log.Debug($"Attempting to establish connection for session {Name}({ID}) to {IPAddress}:{Port} on interface {selectedInterface}.", LoggingCategory);

            Api = new Titan(IPAddress, Port, PortInteractive ?? -1, UseHttps, selectedInterface);
            Api!.Start();

            lock (_sync)
            {
                if (_keepAliveTimer is not null)
                {
                    return;
                }

                _keepAliveTimer = new System.Threading.Timer(
                    KeepAliveCallback,
                    state: null,
                    dueTime: TimeSpan.Zero,
                    period: TimeSpan.FromSeconds(KeepAlive));
            }
        }

        public async Task Stop()
        {
            if (State != SessionConnectionState.Connected && State != SessionConnectionState.Connecting)
                return;

            StopKeepAliveChecks();

            await Api!.StopAsync().ConfigureAwait(false);

            Interlocked.Exchange(ref _consecutiveFailures, 0);

            if (_isEnabled)
                SetState(SessionConnectionState.Enabled);
            else
                SetState(SessionConnectionState.Disabled);

            Log.Debug(
                $"Stopped session {Name}({ID}).",
                LoggingCategory);
        }

        private async void KeepAliveCallback(object? state)
        {
            // Prevent overlapping keep-alive calls if one takes longer
            // than the configured timer interval.
            if (Interlocked.Exchange(ref _keepAliveRunning, 1) == 1)
            {
                return;
            }

            try
            {
                Log.Debug($"Keep-alive check for session {Name}({ID}).", LoggingCategory);
                var connected = await Api!.IsConnected();

                if (connected)
                {
                    Interlocked.Exchange(ref _consecutiveFailures, 0);

                    LastSuccessfulKeepAlive = DateTimeOffset.UtcNow;
                    ComputerName = Api.ConnectedDevice!.ComputerName!;

                    if (!wasConnected)
                        ConnectedAt = DateTime.Now;

                    wasConnected = true;

                    SetState(SessionConnectionState.Connected);

                    Log.Debug(
                        $"Session {Name}({ID}) is connected.",
                        LoggingCategory);
                }
                else
                {
                    RegisterFailure();
                }
            }
            catch (Exception exception)
            {
                RegisterFailure(exception);
            }
            finally
            {
                Interlocked.Exchange(ref _keepAliveRunning, 0);
            }
        }

        private void RegisterFailure(Exception? exception = null)
        {
            int failures = Interlocked.Increment(ref _consecutiveFailures);

            if (wasConnected) SetState(SessionConnectionState.Connecting);

            if (exception is null)
            {
                Log.Warning(
                    $"Keep-alive check failed for session {Name}({ID}). " +
                    $"Attempt {failures} of {ReconnectIterations}.",
                    LoggingCategory);
            }
            else
            {
                Log.Error(
                    exception,
                    $"Keep-alive check failed for session {Name}({ID}). " +
                    $"Attempt {failures} of {reconnectIterations}.",
                    LoggingCategory);
            }

            if (failures < ReconnectIterations)
                return;

            StopKeepAliveChecks();

            SetState(
                SessionConnectionState.Unreachable,
                exception);

            _unreachableReset.Start();

            Log.Warning(
                $"Session {Name}({ID}) was marked disconnected after " +
                $"{failures} consecutive keep-alive failures. " +
                "Automatic keep-alive checks have stopped.",
                LoggingCategory);
        }

        private void StopKeepAliveChecks()
        {
            System.Threading.Timer? timer;

            lock (_sync)
            {
                timer = _keepAliveTimer;
                _keepAliveTimer = null;
            }

            ConnectedAt = null;
            wasConnected = false;

            timer?.Dispose();
            Interlocked.Exchange(ref _consecutiveFailures, 0);
        }

        private void SetState(
            SessionConnectionState newState,
            Exception? exception = null)
        {
            SessionConnectionState previousState;

            lock (_sync)
            {
                if (_state == newState)
                {
                    return;
                }

                previousState = _state;
                _state = newState;
            }

            StateChanged?.Invoke(
                this,
                new SessionStateChangedEventArgs(
                    previousState,
                    newState,
                    exception));

            OnPropertyChanged(nameof(State));
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            await Stop();

            if (Api is IDisposable disposableApi)
            {
                disposableApi.Dispose();
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public void Enable()
        {
            if (State != SessionConnectionState.Disabled)
                return;

            SetState(SessionConnectionState.Enabled);
            _isEnabled = true;
        }

        public void Disable()
        {
            if (State != SessionConnectionState.Enabled 
             && State != SessionConnectionState.Unreachable)
                return;

            SetState(SessionConnectionState.Disabled);
            _isEnabled = false;
        }

        public void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public ISaveModel ToModel()
        {
            return new SessionSaveModel
            {
                ID = ID,
                Name = Name,
                IPAddress = IPAddress,
                Port = Port,
                PortInteractive = PortInteractive,
                UseHttps = UseHttps,
                AutoTimeout = AutoTimeout,
                KeepAlive = KeepAlive,
                ReconnectIterations = ReconnectIterations
            };
        }
    }
}
