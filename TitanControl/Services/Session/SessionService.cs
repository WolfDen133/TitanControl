using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanControl.Disk;
using TitanControl.Disk.Resporitory.Session;
using TitanControl.Logging;
using TitanControl.Models.Session;
using TitanControl.Services.Workspace;

namespace TitanControl.Services.Session
{
    public class SessionService : ISessionService, IAsyncDisposable
    {
        public const string LoggingCategory = "Session Service";

        private readonly SynchronizationContext? _synchronizationContext;

        private readonly ObservableCollection<ISession> _scanResults = new();
        private readonly ReadOnlyObservableCollection<ISession> _readOnlyScanResults;
        private readonly WorkspaceService _workspaceService;
        private readonly SessionRepository _sessionRepository;

        private SessionScanner? _scanner;
        private TimeSpan _scannerDuration;
        private TimeSpan _scannerConnectionTimeout;
        private TimeSpan _scannerDelay;
        private int _scannerConcurrency;
        private bool _scannerUseHttps;

        private ISession? _currentSession;

        private bool _disposed;

        /// <summary>
        /// Configured sessions. Keep this collection instance for the lifetime
        /// of SessionManager; Load() should Clear/Add rather than replace it.
        /// </summary>
        public ObservableCollection<TitanSession> Sessions { get; } = new();

        public ISession? CurrentSession
        {
            get => _currentSession;
            private set
            {
                if (value != null)
                {
                    _currentSession = value;
                    OnPropertyChanged(nameof(CurrentSession));
                }
            }
        }

        /// <summary>
        /// Newly discovered sessions. This collection object is also stable for
        /// the lifetime of SessionManager, even if the scanner/NIC is replaced.
        /// </summary>
        public ReadOnlyObservableCollection<ISession> ScanResults =>
            _readOnlyScanResults;

        public bool IsScannerRunning =>
            _scanner?.IsRunning == true;

        public bool IsScannerConfigured =>
            _scanner is not null;

        public IPAddress? ScannerInterfaceAddress =>
            _scanner?.LocalInterfaceAddress;

        /// <summary>
        /// Forwarded from the manager-owned SessionScanner.
        /// </summary>
        public event EventHandler<bool>? ScannerRunningChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public SessionService(SessionRepository repo, WorkspaceService workspaceService)
        {
            _synchronizationContext = SynchronizationContext.Current;

            _readOnlyScanResults =
                new ReadOnlyObservableCollection<ISession>(_scanResults);

            _sessionRepository = repo;
            _workspaceService = workspaceService;
        }

        public async Task<TitanSession> Create(string name)
        {
            ThrowIfDisposed();

            var session = new TitanSession(
                Guid.NewGuid(),
                name);

            Sessions.Add(session);

            Log.Information(
                $"Created new session '{name}' ({session.ID}).",
                LoggingCategory);

            return session;
        }

        public async Task Delete(Guid sessionId)
        {
            ThrowIfDisposed();

            TitanSession? session =
                Sessions.FirstOrDefault(s => s.ID == sessionId);

            if (session is null)
            {
                var ex = new InvalidOperationException("Session could not be found.");
                Log.Error(ex, $"The session with ID {sessionId} does not exist in the collection.", LoggingCategory);
                throw ex;
            }

            session.Dispose();
            Sessions.Remove(session);

            Log.Information(
                $"Removed session '{sessionId}'.",
                LoggingCategory);
        }


        public async Task<TitanSession> Get(Guid id)
        {
            ThrowIfDisposed();

            var session = Sessions.FirstOrDefault(s => s.ID == id);

            if (session is null)
            {
                var ex = new InvalidOperationException("Session could not be found.");
                Log.Error(ex, $"The session with ID {id} does not exist in the collection.", LoggingCategory);
                throw ex;
            }

            return session;
        }

        public async Task SaveAsync()
        {
            ThrowIfDisposed();

            var record = new SessionRecordModel
            {
                LocalInterface = ScannerInterfaceAddress,
                ScannerDuration = _scannerDuration.Seconds,
                ScannerConcurrency = _scannerConcurrency,
                ScannerTimeout = _scannerConnectionTimeout.Milliseconds,
                ScannerDelay = _scannerDelay.Seconds,
                ScannerUseHttps = _scannerUseHttps,
                Sessions = Sessions.Select(s => (SessionSaveModel)s.ToModel()).ToList()
            };

            await _sessionRepository.SaveAsync(record);

            Log.Information($"Saved {Sessions.Count} session(s).", LoggingCategory);
        }

        public async Task LoadAsync()
        {
            ThrowIfDisposed();

            Sessions.Clear();

            var sessionRecord = await _sessionRepository.LoadAsync();

            await ConfigureScannerAsync(
                sessionRecord.LocalInterface ?? IPAddress.Loopback,
                TimeSpan.FromSeconds(sessionRecord.ScannerDuration),
                TimeSpan.FromMilliseconds(sessionRecord.ScannerTimeout),
                TimeSpan.FromSeconds(sessionRecord.ScannerDelay),
                sessionRecord.ScannerConcurrency,
                sessionRecord.ScannerUseHttps);

            foreach (var session in sessionRecord.Sessions)
            {
                Sessions.Add((TitanSession)session.ToInstance());
            }

            Log.Information($"Loaded {Sessions.Count} session(s).", LoggingCategory);
        }


        /// <summary>
        /// Creates/replaces the manager-owned scanner when its configuration
        /// actually changes. Calling this again with the same settings is a no-op,
        /// which means recreating SessionPageModel does not recreate the scanner.
        /// </summary>
        public async Task ConfigureScannerAsync(
            IPAddress localInterfaceAddress,
            TimeSpan? scanDuration = null,
            TimeSpan? connectionTimeout = null,
            TimeSpan? delayBetweenScans = null,
            int maximumConcurrency = 64,
            bool useHttps = false)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(localInterfaceAddress);

            TimeSpan duration =
                scanDuration ?? TimeSpan.FromSeconds(30);
            TimeSpan timeout =
                connectionTimeout ?? TimeSpan.FromMilliseconds(300);
            TimeSpan delay =
                delayBetweenScans ?? TimeSpan.FromSeconds(4);

            bool sameConfiguration =
                _scanner is not null &&
                _scanner.LocalInterfaceAddress.Equals(localInterfaceAddress) &&
                _scannerDuration == duration &&
                _scannerConnectionTimeout == timeout &&
                _scannerDelay == delay &&
                _scannerConcurrency == maximumConcurrency &&
                _scannerUseHttps == useHttps;

            if (sameConfiguration)
                return;

            if (_scanner is not null)
            {
                DetachScanner(_scanner);
                _scanner.Stop();
                await _scanner.DisposeAsync();
                _scanner = null;
            }

            // Results from another NIC are no longer meaningful for the newly
            // selected network.
            await RunOnCapturedContextAsync(_scanResults.Clear);

            _scanner = new SessionScanner(
                localInterfaceAddress,
                duration,
                timeout,
                delay,
                maximumConcurrency,
                useHttps);

            _scannerDuration = duration;
            _scannerConnectionTimeout = timeout;
            _scannerDelay = delay;
            _scannerConcurrency = maximumConcurrency;
            _scannerUseHttps = useHttps;

            AttachScanner(_scanner);

            Log.Information(
                $"Configured session scanner on {localInterfaceAddress}.",
                LoggingCategory);
        }

        /// <summary>
        /// Starts scanning against the manager's persistent Sessions collection.
        /// </summary>
        public async Task StartScannerAsync(
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            SessionScanner scanner = GetScanner();

            await scanner.StartAsync(
                Sessions,
                cancellationToken);
        }

        public void StopScanner()
        {
            ThrowIfDisposed();
            _scanner?.Stop();
        }

        public async Task ClearScanResultsAsync()
        {
            ThrowIfDisposed();

            if (_scanner is not null)
                await _scanner.ClearResultsAsync();

            await RunOnCapturedContextAsync(_scanResults.Clear);
        }

        private void AttachScanner(SessionScanner scanner)
        {
            scanner.OnScannerRunning += ScannerRunning;
            scanner.ResultsChanged += ScannerResultsChanged;

            // Normally empty for a new scanner, but keeps this correct if the
            // scanner implementation later starts with existing results.
            SyncAllScannerResults(scanner);
        }

        private void DetachScanner(SessionScanner scanner)
        {
            scanner.OnScannerRunning -= ScannerRunning;
            scanner.ResultsChanged -= ScannerResultsChanged;
        }

        private void ScannerRunning(object? sender, bool running)
        {
            ScannerRunningChanged?.Invoke(this, running);
        }

        /// <summary>
        /// Mirrors SessionScanner.Results into a stable manager-owned collection.
        /// This means the UI never has to replace its ItemsSource when the scanner
        /// instance changes.
        /// </summary>
        private void ScannerResultsChanged(
            object? sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (_synchronizationContext is not null &&
                SynchronizationContext.Current != _synchronizationContext)
            {
                _synchronizationContext.Post(
                    _ => ApplyScannerResultsChange(e),
                    null);
                return;
            }

            ApplyScannerResultsChange(e);
        }

        private void ApplyScannerResultsChange(
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems is not null)
                    {
                        foreach (ISession session in e.NewItems)
                        {
                            if (!_scanResults.Contains(session))
                                _scanResults.Add(session);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems is not null)
                    {
                        foreach (ISession session in e.OldItems)
                            _scanResults.Remove(session);
                    }
                    break;

                default:
                    if (_scanner is not null)
                        SyncAllScannerResults(_scanner);
                    else
                        _scanResults.Clear();
                    break;
            }
        }

        private void SyncAllScannerResults(SessionScanner scanner)
        {
            _scanResults.Clear();

            foreach (ISession session in scanner.Results)
                _scanResults.Add(session);
        }

        private SessionScanner GetScanner()
        {
            return _scanner ?? throw new InvalidOperationException(
                "The session scanner has not been configured. " +
                "Call ConfigureScannerAsync first.");
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
                        completion.SetException(exception);
                    }
                },
                null);

            return completion.Task;
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        /// <summary>
        /// Prefer this during application shutdown because SessionScanner owns
        /// asynchronous resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_scanner is not null)
            {
                DetachScanner(_scanner);
                _scanner.Stop();
                await _scanner.DisposeAsync();
                _scanner = null;
            }

            foreach (ISession session in Sessions.ToArray())
                session.Dispose();

            Sessions.Clear();
            _scanResults.Clear();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Kept for IDisposable/ISessionManager compatibility. Prefer
        /// DisposeAsync at application shutdown so the scanner is fully awaited.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_scanner is not null)
            {
                DetachScanner(_scanner);
                _scanner.Stop();
            }

            foreach (ISession session in Sessions.ToArray())
                session.Dispose();

            Sessions.Clear();
            _scanResults.Clear();

            GC.SuppressFinalize(this);
        }


        public void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
    }
}
