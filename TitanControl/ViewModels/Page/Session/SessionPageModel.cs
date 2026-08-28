using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using TitanControl.Events.Control;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Models.Workspace;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModel;
using TitanControl.ViewModels.Page.Session;
using TitanControl.WebAPI.Data.Model;

namespace TitanControl.ViewModels.Page
{
    public partial class SessionPageModel : BasePageModel
    {
        private const string LoggingCategory = "SessionPageModel";

        private int _selectedNic;
        private bool _refreshEnabled;
        public bool _editing = false;
        private bool _selectingSession;
        private ISession? _selectedSession;
        private IWorkspaceService _workspaceService;
        private ISessionService _sessionService;
        private SessionFormModel? _formModel;
        private List<IPAddress> _ipAddresses = new();

        public override PageId Id => PageId.Session;

        public ISession? SelectedSession
        {
            get => _selectedSession;
            set
            {
                _selectedSession = value;
                
                OnPropertyChanged(nameof(SelectedSession));
                OnPropertyChanged(nameof(DisplaySession));
                OnPropertyChanged(nameof(DetailsEnabled));
                OnPropertyChanged(nameof(Endpoint));
                OnPropertyChanged(nameof(ShowSwap));
                OnPropertyChanged(nameof(ShowDisable));
            }
        }

        public bool RefreshEnabled
        {
            get => _refreshEnabled;
            set
            {
                _refreshEnabled = value;
                OnPropertyChanged(nameof(RefreshEnabled));
            }
        }

        public int SelectedNic
        {
            get => _selectedNic;
            set
            {
                _selectedNic = value;
                _ = RegisterScanner(true);

                OnPropertyChanged(nameof(SelectedNic));
            }
        }

        public bool IsEditing
        {
            get => _editing;
            set
            {
                _editing = value;
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(DetailsEnabled));
            }
        }

        public SessionFormModel? FormData
        {
            get => _formModel;
            set
            {
                _formModel = value;
                OnPropertyChanged(nameof(FormData));
            }
        }

        public ObservableCollection<string> Interfaces { get; private set; } = new();
        

        public string Endpoint =>
         EnabledSession is not null
         ? (EnabledSession.PortInteractive is not null
             ? $"{EnabledSession!.IPAddress} : {EnabledSession!.Port}/{EnabledSession!.PortInteractive!}"
             : $"{EnabledSession!.IPAddress} : {EnabledSession!.Port}")
         : "-";

        public WorkspaceModel CurrentWorkspace => _workspaceService.CurrentWorkspace;
        public ObservableCollection<TitanSession> Sessions => _sessionService.Sessions;
        public ISession? EnabledSession => _sessionService.CurrentSession;
        public ReadOnlyObservableCollection<ISession> ScanResults => _sessionService.ScanResults;
        public ConnectedDevice Device { get; } = new();

        public bool HasNoSessions => Sessions.Count == 0;
        public bool DetailsEnabled => (SelectedSession != null || EnabledSession != null) && !IsEditing;
        public ISession? DisplaySession => SelectedSession ?? EnabledSession;
        private bool SessionsIdentical => EnabledSession?.ID == SelectedSession?.ID;

        public bool ShowSwap =>
            !SessionsIdentical
            && SelectedSession != null
            && EnabledSession?.State == SessionConnectionState.Enabled;

        public bool ShowDisable =>
            (SessionsIdentical || SelectedSession == null) 
            && EnabledSession?.State == SessionConnectionState.Enabled;

        public SessionPageModel(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            _sessionService = sessionService;
            _workspaceService = workspaceService;

            sessionService.ScannerRunningChanged += (_, isRunning) =>
            {
                RefreshEnabled = !isRunning;
            };

            Sessions.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasNoSessions));
            };

            sessionService.PropertyChanged += SessionService_PropertyChanged;
        }

        /*
         * Lifecycle events
         */
        public override async Task InitializeAsync()
        {
            var defaultIp = _sessionService.ScannerInterfaceAddress ?? NicHelper.GetDefaultIPv4Address();

            foreach (var nic in _sessionService.Nics)
            {
                Interfaces.Add($"{nic.Key} - {nic.Value}");
                _ipAddresses.Add(nic.Value);

                Log.Debug($"Discovered network interface {nic.Key} - {nic.Value}", LoggingCategory);

                if (!nic.Value.Equals(defaultIp))
                    continue;

                _selectedNic = _ipAddresses.IndexOf(defaultIp);
                OnPropertyChanged(nameof(SelectedNic));
                await RegisterScanner();

                Log.Debug($"Default network interface selected {nic.Key} - {defaultIp}", LoggingCategory);
            }
        }

        public override Task OnOpenAsync()
        {
            StartScanner();

            return Task.CompletedTask;
        }

        public override Task OnCloseAsync()
        {
            StopScanner();

            return Task.CompletedTask;
        }

        /*
         * Event Listeners
         */

        // Listen for enabled session changes
        private void SessionService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(_sessionService.CurrentSession))
                return;

            EnabledSession?.StateChanged += EnabledSession_StateChanged;
            OnPropertyChanged(nameof(EnabledSession));
            OnPropertyChanged(nameof(DisplaySession));
            OnPropertyChanged(nameof(DetailsEnabled));
            OnPropertyChanged(nameof(Endpoint));
            OnPropertyChanged(nameof(ShowSwap));
            OnPropertyChanged(nameof(ShowDisable));
        }

        // Listen for the enabled session's state change
        private void EnabledSession_StateChanged(object? sender, Events.Session.SessionStateChangedEventArgs e)
        {
            if (e.CurrentState == SessionConnectionState.Connected)
            {
                Device.ComputerName = EnabledSession!.Api!.ConnectedDevice!.ComputerName;
                Device.Legend = EnabledSession!.Api!.ConnectedDevice!.Legend;
                Device.SoftwareVersion = EnabledSession!.Api!.ConnectedDevice!.SoftwareVersion?.ToString();
                Device.Id = EnabledSession!.Api!.ConnectedDevice!.Id;
                Device.ConnectedTo = EnabledSession!.Api!.ConnectedDevice!.ConnectedTo;
                Device.Notes = EnabledSession!.Api!.ConnectedDevice!.Notes;
            }
            else
                Device.Clear();

            OnPropertyChanged(nameof(Device));
            OnPropertyChanged(nameof(ShowSwap));
            OnPropertyChanged(nameof(ShowDisable));
        }

        // Listen for the session selection
        public async void HandleSessionSelect(object? sender, SessionOverviewSelectedEventArgs e)
        {
            if (_selectingSession)
                return;

            _selectingSession = true;

            try
            {
                var session = Sessions.FirstOrDefault(s => s.ID == e.SessionId);

                if (session == null)
                {
                    if (ScanResults.Any(s => s.ID == e.SessionId))
                        return;

                    Log.Error(
                        $"Selected session {e.SessionId} was not found in list",
                        LoggingCategory);

                    return;
                }

                if (IsEditing)
                {
                    var accepted = await App.DialogService
                        .ShowConfirmationAsync(
                            "Unsaved changes",
                            "Are you sure you wish to cancel? All changes will not be saved.");

                    if (!accepted)
                        return;

                    DisableForm();
                }

                if (SelectedSession?.ID == session.ID && session.IsSelected)
                {
                    ReleaseSelect(session);
                    return;
                }

                foreach (var s in Sessions)
                {
                    s.IsSelected = s.ID == session.ID;
                }

                SelectedSession = session;
                Log.Information($"Selected session: {session.Name}", LoggingCategory);
            }
            finally
            {
                _selectingSession = false;
            }
        }

        /*
         * Helper functions
         * Various functions to configure and modify the view 
         */

        public async Task RegisterScanner(bool start = false)
        {
            if (_sessionService.Nics.Count == 0)
                return;

            await _sessionService.ConfigureScannerAsync(
                _ipAddresses[SelectedNic],
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMilliseconds(300),
                TimeSpan.FromSeconds(4),
                64,
                false);

            await UpdateSessions();

            if (start)
            {
                _ = _sessionService.SaveAsync();
                _ = _sessionService.StartScannerAsync();
            }  
        }


        public void StartScanner()
        {
            _ = _sessionService.StartScannerAsync();
        }

        public void StopScanner()
        {
            _sessionService.StopScanner();
        }

        private async Task UpdateSessions()
        {
            foreach (var session in Sessions)
            {
                if (session.State != SessionConnectionState.Connected)
                    return;

                await session.Stop();
                await session.Start(_ipAddresses[_selectedNic]);
            }
        }

        public void ReleaseSelect(ISession session)
        {
            session.IsSelected = false;
            SelectedSession = null;
        }

        private void EnableForm()
        {
            if (SelectedSession is null)
            {
                var ex = new InvalidOperationException("Selected session is null.");
                Log.Error(ex, "Selected session is invalid", LoggingCategory);
                return;
            }


            FormData = SessionFormModel.FromModel(SelectedSession);
            IsEditing = true;
        }

        private void DisableForm()
        {
            FormData = SessionFormModel.Empty;
            IsEditing = false;

            OnPropertyChanged(nameof(FormData));
        }

        /*
         * Relay Commands
         * All the functionality for the buttons
         */

        [RelayCommand]
        public async Task SessionQuickAction(Guid sessionId)
        {
            ISession? session = Sessions.FirstOrDefault(s => s.ID == sessionId);
            session ??= ScanResults.FirstOrDefault(s => s.ID == sessionId);

            if (session == null)
            {
                var ex = new InvalidOperationException("The session was not found");
                Log.Error(ex, "Could not find the quick action session to modify", LoggingCategory);
                return;
            }

            Log.Debug($"Executing quick action for {session.Name} at state {session.State}", LoggingCategory);

            switch (session.State)
            {
                case SessionConnectionState.Available:
                    var apiSession = await _sessionService.Create(session.Name);
                    apiSession.IPAddress = session.IPAddress;
                    apiSession.Port = session.Port;
                    apiSession.PortInteractive = session.PortInteractive;
                    apiSession.UseHttps = session.UseHttps;
                    apiSession.ReconnectIterations = session.ReconnectIterations;
                    apiSession.KeepAlive = session.KeepAlive;
                    apiSession.AutoTimeout = session.AutoTimeout;

                    break;

                case SessionConnectionState.Unreachable:

                    await Connect(sessionId);
                    break;

                case SessionConnectionState.Connected:

                    await Disconnect(sessionId);
                    break;

                case SessionConnectionState.Disabled:

                    await _sessionService.Select(sessionId);
                    break;

                case SessionConnectionState.Enabled:

                    await Connect(sessionId);
                    break;
            }

            
        }

        [RelayCommand]
        public async Task Enable()
        {
            await _sessionService.Select(SelectedSession!.ID);
        }

        [RelayCommand]
        public async Task Disable()
        {
            await _sessionService.Select(Guid.Empty);
        }


        [RelayCommand]
        public async Task AddManual()
        {
            var session = await _sessionService.Create(PathHelper.GetNextFileName("Titan Session", Sessions.Select(s => s.Name)));

            if (IsEditing)
                return;

            foreach (var s in Sessions)
            {
                s.IsSelected = s.ID == session.ID;
            }

            SelectedSession = session;
            EnableForm();

            await _sessionService.SaveAsync();

            Log.Information("Added a new session manually", LoggingCategory);
        }

        [RelayCommand]
        public async Task SaveSession()
        {
            ISession? session = _sessionService.Sessions.FirstOrDefault(s => s.ID == SelectedSession!.ID);

            if (session == null)
            {
                Log.Error("Session could not be found", LoggingCategory);
                return;
            }

            if (FormData == null)
            {
                Log.Error("Form data is null", LoggingCategory);
                return;
            }

            session.Name = FormData.SessionName;
            session.IPAddress = IPAddress.Parse(FormData.IpAddress);
            session.Port = FormData.Port;
            session.PortInteractive = FormData.PortInteractive;
            session.UseHttps = FormData.UseHttps;
            session.ReconnectIterations = FormData.Reconnect ? (int)FormData.ReconnectAttempts! : 0;
            session.KeepAlive = FormData.KeepAliveSeconds;
            session.AutoTimeout = FormData.AutoTimeout ? FormData.AutoTimeoutMinuates : null;

            OnPropertyChanged(nameof(Sessions));
            SelectedSession = session;

            DisableForm();

            await _sessionService.SaveAsync();

            Log.Information($"Saved session {session.Name}.", LoggingCategory);
        }

        [RelayCommand]
        public async Task Cancel()
        {
            if (!FormData!.Equals(SelectedSession))
            {
                var accepted = await App.DialogService.ShowConfirmationAsync("Unsaved changes", "Are you sure you wish to cancel, all changes will not be saved?");

                if (!accepted)
                    return;
            }

            DisableForm();
        }


        [RelayCommand]
        public async Task Connect(Guid? sessionId = null)
        {
            if (sessionId == null)
            {
                if (EnabledSession == null)
                    return;

                sessionId = EnabledSession.ID;
            }

            var session = _sessionService.Sessions.FirstOrDefault(s => s.ID == sessionId);
            
            if (session is null)
            {
                Log.Warning($"Session {sessionId} not found.", LoggingCategory);
                return;
            }


            Log.Information($"Attempting to start session: {session.Name}", LoggingCategory);

            _ = session.Start(_ipAddresses[_selectedNic]);
        }


        [RelayCommand]
        public async Task Disconnect(Guid? sessionId = null)
        {
            if (sessionId == null)
            {
                if (EnabledSession == null)
                    return;

                sessionId = EnabledSession.ID;
            }

            var session = Sessions.FirstOrDefault(s => s.ID == sessionId);

            if (session is null)
            {
                Log.Warning($"Session {sessionId} not found.", LoggingCategory);
                return;
            }

            Log.Information($"Attempting to stop session: {session.Name}", LoggingCategory);

            await session.Stop();
        }


        [RelayCommand]
        public async Task Edit()
        {
            Log.Debug($"Hit edit command");
            EnableForm();
        }


        [RelayCommand]
        public async Task Duplicate(Guid? sessionId = null)
        {
            if (sessionId == null)
            {
                if (EnabledSession == null)
                    return;

                sessionId = EnabledSession.ID;
            }

            var session = _sessionService.Sessions.FirstOrDefault(s => s.ID == sessionId);

            if (session is null)
            {
                Log.Warning($"Session {sessionId} not found.", LoggingCategory);
                return;
            }

            var newSession = await _sessionService.Create(PathHelper.GetNextFileName(session.Name, Sessions.Select(s => s.Name)));
            newSession.IPAddress = session.IPAddress;
            newSession.Port = session.Port;
            newSession.PortInteractive = session.PortInteractive;
            newSession.UseHttps = session.UseHttps;
            newSession.ReconnectIterations = session.ReconnectIterations;
            newSession.KeepAlive = session.KeepAlive;
            newSession.AutoTimeout = session.AutoTimeout;

            Log.Information($"Duplicated session: {session.Name}", LoggingCategory);

            await _sessionService.SaveAsync();

            if (IsEditing)
                return;

            foreach (var s in Sessions)
            {
                s.IsSelected = s.ID == newSession.ID;
            }

            SelectedSession = newSession;
        }

        [RelayCommand]
        public async Task Remove(Guid? sessionId = null)
        {
            if (sessionId == null)
            {
                if (EnabledSession == null)
                    return;

                sessionId = EnabledSession.ID;
            }

            var session = _sessionService.Sessions.FirstOrDefault(s => s.ID == sessionId);

            if (session is null)
            {
                Log.Warning($"Session {sessionId} not found.", LoggingCategory);
                return;
            }

            var accepted = await App.DialogService
                        .ShowConfirmationAsync(
                            "Remove session",
                            $"Are you sure you wish to remove the session: {session.Name}.\nThis action cannot be undone.");

            if (!accepted)
                return;

            DisableForm();

            if (EnabledSession?.ID == session.ID)
                await _sessionService.Select(Guid.Empty);

            await _sessionService.Delete((Guid)sessionId);

            SelectedSession = null;

            Log.Information($"Removed session: {session.Name}", LoggingCategory);

            await _sessionService.SaveAsync();
        }

        public override ValueTask DisposeAsync()
        {
            Interfaces.Clear();
            _ipAddresses.Clear();

            _selectedNic = -1;
            _selectedSession = null;
            _formModel = null;

            _sessionService.StopScanner();

            return ValueTask.CompletedTask;
        }
    }
}
