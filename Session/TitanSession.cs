using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanControl.Session.Interface;
using TitanWebAPI;

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
            UUID id,
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

        private UUID Id { get; }
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

        public UUID ID => Id;

        public bool UseHttps { get; set; }
        public DateTimeOffset? LastSucessfulKeepAlive => LastSuccessfulKeepAlive;

        public string Name { get; set; } = string.Empty;

        public event EventHandler<SessionStateChangedEventArgs>? StateChanged;

        public void Start()
        {
            SetState(SessionConnectionState.Connecting);
            ThrowIfDisposed();
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
            Timer? timer;

            lock (_sync)
            {
                timer = _keepAliveTimer;
                _keepAliveTimer = null;
            }

            Api.Stop();
            timer?.Dispose();

            SetState(SessionConnectionState.Disconnected);
        }

        public void SetDetails(IPAddress ipAddress, int port = 4430, bool useHttps = false)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(ipAddress);

            Api.Stop();
            Api.Dispose();

            Api = (TApi) new Titan(ipAddress, port, useHttps);
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
                var connected = await Api.IsConnected();

                if (connected)
                {
                    _consecutiveFailures = 0;
                    LastSuccessfulKeepAlive = DateTimeOffset.UtcNow;

                    SetState(SessionConnectionState.Connected);
                }
                else
                {
                    RegisterFailure();
                }
            }
            catch (Exception exception)
            {
                RegisterFailure(exception);
                SetState(SessionConnectionState.Error);
            }
            finally
            {
                Interlocked.Exchange(ref _keepAliveRunning, 0);
            }
        }

        private void RegisterFailure(Exception? exception = null)
        {
            var failures = Interlocked.Increment(
                ref _consecutiveFailures);

            if (failures >= _options.FailuresBeforeDisconnected)
            {
                SetState(
                    SessionConnectionState.Disconnected,
                    exception);
            }
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
