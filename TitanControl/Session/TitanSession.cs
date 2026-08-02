using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Session.Interface;
using TitanControl.WebAPI;

namespace TitanControl.Session
{
    public sealed class ApiSession<TApi> : ISession<TApi>
    where TApi : Titan
    {
        private readonly SessionOptions _options;
        private readonly object _sync = new();

        private Timer? _keepAliveTimer;
        private SessionConnectionState _state =
            SessionConnectionState.Disconnected;

        private int _consecutiveFailures;
        private int _keepAliveRunning;
        private bool _disposed;

        public ApiSession(
            Guid id,
            TApi api,
            SessionOptions options)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(api);
            ArgumentNullException.ThrowIfNull(options);

            if (options.KeepAliveInterval <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.KeepAliveInterval));
            }

            if (options.FailuresBeforeDisconnected < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.FailuresBeforeDisconnected));
            }

            Id = id;
            Api = api;
            _options = options;
        }

        private Guid Id { get; }
        public TApi Api { get; private set; }

        public SessionConnectionState State
        {
            get
            {
                lock (_sync)
                {
                    return _state;
                }
            }
        }

        public bool IsConnected =>
            State == SessionConnectionState.Connected;

        public DateTimeOffset? LastSuccessfulKeepAlive { get; private set; }

        public Guid ID => Id;

        public bool UseHttps { get; set; }
        public DateTimeOffset? LastSucessfulKeepAlive => LastSuccessfulKeepAlive;

        public string Name { get; set; } = string.Empty;

        public event EventHandler<SessionStateChangedEventArgs>? StateChanged;

        private bool wasConnected;

        public void Start()
        {
            Log.Debug($"Starting session {Name}({Id}).", "SessionInstance");

            SetState(SessionConnectionState.Connecting);
            ThrowIfDisposed();

            Log.Debug($"Attempting to establish connection for session {Name}({Id}).", "SessionInstance");

            Api.Start();

            lock (_sync)
            {
                if (_keepAliveTimer is not null)
                {
                    return;
                }

                _keepAliveTimer = new Timer(
                    KeepAliveCallback,
                    state: null,
                    dueTime: TimeSpan.Zero,
                    period: _options.KeepAliveInterval);
            }
        }

        public void Stop()
        {
            StopKeepAliveChecks();

            Api.Stop();

            Interlocked.Exchange(ref _consecutiveFailures, 0);

            SetState(SessionConnectionState.Disconnected);

            Log.Debug(
                $"Stopped session {Name}({Id}).",
                "SessionInstance");
        }

        public void SetDetails(IPAddress ipAddress, int port = 4430, bool useHttps = false)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(ipAddress);

            Api.Stop();
            Api.Dispose();

            Api = (TApi) new Titan(ipAddress, port);
            Api.Start();
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
                Log.Debug($"Keep-alive check for session {Name}({Id}).", "SessionInstance");
                var connected = await Api.IsConnected();

                if (connected)
                {
                    Interlocked.Exchange(ref _consecutiveFailures, 0);

                    LastSuccessfulKeepAlive = DateTimeOffset.UtcNow;

                    SetState(SessionConnectionState.Connected);
                    wasConnected = true;

                    Log.Debug(
                        $"Session {Name}({Id}) is connected.",
                        "SessionInstance");
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
                    $"Keep-alive check failed for session {Name}({Id}). " +
                    $"Attempt {failures} of {_options.FailuresBeforeDisconnected}.",
                    "SessionInstance");
            }
            else
            {
                Log.Error(
                    exception,
                    $"Keep-alive check failed for session {Name}({Id}). " +
                    $"Attempt {failures} of {_options.FailuresBeforeDisconnected}.",
                    "SessionInstance");
            }

            if (failures < _options.FailuresBeforeDisconnected)
                return;

            StopKeepAliveChecks();

            SetState(
                SessionConnectionState.Disconnected,
                exception);

            Log.Warning(
                $"Session {Name}({Id}) was marked disconnected after " +
                $"{failures} consecutive keep-alive failures. " +
                "Automatic keep-alive checks have stopped.",
                "SessionInstance");
        }

        private void StopKeepAliveChecks()
        {
            Timer? timer;

            lock (_sync)
            {
                timer = _keepAliveTimer;
                _keepAliveTimer = null;
            }

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
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Stop();

            if (Api is IDisposable disposableApi)
            {
                disposableApi.Dispose();
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
