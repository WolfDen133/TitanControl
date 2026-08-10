using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TitanControl.Controls.Menu;
using TitanControl.Disk.Model.Session;
using TitanControl.Disk.Model.Workspace;
using TitanControl.Events.Control;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Session;
using TitanControl.Session.Interface;
using TitanControl.Session.Utils;
using TitanControl.ViewModels.Page;
using TitanControl.Views;
using TitanControl.WebAPI;

namespace TitanControl.ViewModels
{
    public partial class SessionPageModel : BaseViewModel, INotifyPropertyChanged
    {
        private const string LoggingCategory = "SessionPageModel";

        private List<KeyValuePair<string, IPAddress>> _nics = new();
        private int selectedNic;
        private bool refreshEnabled;
        public bool editing = false;
        private SessionFormModel? formModel;
        private ISession? selectedSession;

        public ISession? SelectedSession 
        { 
            get => selectedSession; 
            set
            {
                selectedSession = value;
                OnPropertyChanged(nameof(SelectedSession));
                OnPropertyChanged(nameof(Endpoint));
            }
        }

        public bool RefreshEnabled
        {
            get => refreshEnabled;
            set
            {
                refreshEnabled = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(RefreshEnabled));
            }
        }
      
        public int SelectedNic
        {
            get => selectedNic;
            set
            {
                selectedNic = value;
                _ = RegisterScanner(true);

                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedNic));
            }
        }

        public bool IsEditing
        {
            get => editing;
            set
            {
                editing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }

        public SessionFormModel? FormData
        {
            get => formModel;
            set
            {
                formModel = value;
                OnPropertyChanged(nameof(FormData));
            }
        }

        public ObservableCollection<string> Interfaces { get; private set; } = new();
        public ObservableCollection<ISession> EnabledSessions { get; } = new();

        public string Endpoint =>
         SelectedSession is not null
         ? (SelectedSession?.PortInteractive is not null
             ? $"{SelectedSession!.IPAddress} : {SelectedSession!.Port}/{SelectedSession!.PortInteractive!}"
             : $"{SelectedSession!.IPAddress} : {SelectedSession!.Port}")
         : "-";

        public WorkspaceModel CurrentWorkspace => App.WorkspaceManager.CurrentWorkspace;
        public ObservableCollection<TitanSession> Sessions => App.SessionManager.Sessions;
        public ReadOnlyObservableCollection<SessionModel> ScanResults => App.SessionManager.ScanResults;

        public bool HasNoSessions => Sessions.Count == 0;

        public SessionPageModel()
        {
            App.SessionManager.ScannerRunningChanged +=
                (_, isRunning) =>
                {
                    RefreshEnabled = !isRunning;
                };

            Sessions.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasNoSessions));
            };
        }

        public async Task RegisterScanner(bool start = false)
        {
            if (_nics.Count == 0)
                return;

            await App.SessionManager.ConfigureScannerAsync(
                _nics[SelectedNic].Value,
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMilliseconds(300),
                TimeSpan.FromSeconds(4),
                64,
                false);

            UpdateSessions();

            if (start)
                await StartScanner();
        }

        private void UpdateSessions()
        {
            foreach (var session in Sessions)
            {
                if (session.State != SessionConnectionState.Connected)
                    return;

                session.Stop();
                session.Start(_nics[selectedNic].Value);
            }
        }

        public void Initialize()
        {
            Interfaces.Clear();
            _nics.Clear();

            var nics = NicHelper.GetNics();
            var defaultIp = NicHelper.GetDefaultIPv4Address();

            foreach (var nic in nics)
            {
                var nicRecord = new KeyValuePair<string, IPAddress>(nic.Key, nic.Value);
                Interfaces.Add($"{nic.Key} - {nic.Value}");
                _nics.Add(nicRecord);

                Log.Debug($"Discovered network interface {nic.Key} - {nic.Value}", LoggingCategory);

                if (nicRecord.Value.Equals(defaultIp))
                {
                    selectedNic = _nics.IndexOf(nicRecord);
                    OnPropertyChanged(nameof(SelectedNic));

                    _ = RegisterScanner();

                    Log.Debug($"Default network interface selected {nic.Key} - {defaultIp}", LoggingCategory);
                }   
            }
        }

        public async Task StartScanner()
        {
            await App.SessionManager.StartScannerAsync();
        }

        public void StopScanner()
        {
            App.SessionManager.StopScanner();
        }

        private bool _selectingSession;

        public async void HandleSessionSelect(
            object? sender,
            SessionOverviewSelectedEventArgs e)
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
                    var accepted = await MainWindow.DialogService
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

        public void ReleaseSelect(ISession session)
        {
            session.IsSelected = false;
            SelectedSession = App.SessionManager.Sessions.FirstOrDefault(s => s.ID == CurrentWorkspace.Options.Session);
        }

        public async Task Clear()
        {
            await App.SessionManager.ClearScanResultsAsync();
            SelectedSession = null;
        }

        public void EnableSession(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
            {
                CurrentWorkspace.Options.Session = Guid.Empty;
                SelectedSession = Sessions.FirstOrDefault(s => s.IsSelected);
            }
                

            foreach (var s in Sessions)
            {
                if (s.ID == sessionId)
                {
                    s.Enable();
                    CurrentWorkspace.Options.Session = s.ID;
                    EnabledSessions.Add(s);
                    SelectedSession = s;

                    Log.Information($"Enabled session: {s.Name}", LoggingCategory);
                    continue;
                }

                s.Stop();
                s.Enable(false);
                EnabledSessions.Remove(s);
            }

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

        [RelayCommand]
        public async Task SessionQuickAction(Guid sessionId)
        {
            ISession? session = Sessions.FirstOrDefault(s => s.ID == sessionId);
            if (session == null)
                session = ScanResults.FirstOrDefault(s => s.ID == sessionId);


            if (session == null)
            {
                var ex = new InvalidOperationException("The session was not found");
                Log.Error(ex, "Could not find the quick action session to modify", LoggingCategory);
                return;
            }

            switch (session.State)
            {
                case SessionConnectionState.Available:
                    var apiSession = App.SessionManager.Create(session.Name);
                    apiSession.IPAddress = session.IPAddress;
                    apiSession.Port = session.Port;
                    apiSession.PortInteractive = session.PortInteractive;
                    apiSession.UseHttps = session.UseHttps;
                    apiSession.ReconnectIterations = session.ReconnectIterations;
                    apiSession.KeepAlive = session.KeepAlive;
                    apiSession.AutoTimeout = session.AutoTimeout;

                    break;

                case SessionConnectionState.Unreachable:

                    Connect(sessionId);
                    break;

                case SessionConnectionState.Connected:

                    Disconnect(sessionId);
                    break;

                case SessionConnectionState.Disabled:

                    EnableSession(sessionId);
                    break;

                case SessionConnectionState.Enabled:

                    Connect(sessionId);
                    break;
            }
        }

        [RelayCommand]
        public async Task Enable()
        {
            EnableSession(SelectedSession!.ID);
        }

        [RelayCommand]
        public async Task Disable()
        {
            EnableSession(Guid.Empty);
        }


        [RelayCommand]
        public void AddManual()
        {
            var session = App.SessionManager.Create(GetNextName("Titan Session"));

            if (IsEditing)
                return;
           
            foreach (var s in Sessions)
            {
                s.IsSelected = s.ID == session.ID;
            }

            SelectedSession = session;
            EnableForm();

            Log.Information("Added a new session manually", LoggingCategory);
        }

        [RelayCommand]
        public void SaveSession()
        {
            ISession? session = App.SessionManager.Sessions.FirstOrDefault(s => s.ID == SelectedSession!.ID);

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

            if (SelectedSession is not SessionModel)
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

            Log.Information($"Saved session {session.Name}.", LoggingCategory);
        }

        [RelayCommand]
        public async Task Cancel()
        {
            if (!FormData!.Equals(SelectedSession))
            {
                var accepted = await MainWindow.DialogService.ShowConfirmationAsync("Unsaved changes", "Are you sure you wish to cancel, all changes will not be saved?");

                if (!accepted)
                    return;
            }

            DisableForm();
        }


        [RelayCommand]
        public void Connect(Guid? sessionId = null)
        {
            if (sessionId == null)
            {
                if (SelectedSession == null)
                    return;

                sessionId = SelectedSession.ID;
            }

            var session = App.SessionManager.Sessions.FirstOrDefault(s => s.ID == sessionId);

            if (session is null)
            {
                Log.Warning($"Session {sessionId} not found.", LoggingCategory);
                return;
            }

            Log.Information($"Attempting to start session: {session.Name}", LoggingCategory);

            session.Start(_nics[selectedNic].Value);
        }


        [RelayCommand]
        public void Disconnect(Guid? sessionId = null)
        {
            if (sessionId == null)
            {
                if (SelectedSession == null)
                    return;

                sessionId = SelectedSession.ID;
            }

            var session = App.SessionManager.Sessions.FirstOrDefault(s => s.ID == sessionId);

            if (session is null)
            {
                Log.Warning($"Session {sessionId} not found.", LoggingCategory);
                return;
            }

            Log.Information($"Attempting to stop session: {session.Name}", LoggingCategory);

            App.SessionManager.Sessions.FirstOrDefault(s => s.ID == sessionId)!.Stop();
        }


        [RelayCommand]
        public void Edit()
        {
            Log.Debug($"Hit edit command");
            EnableForm();
        }


        [RelayCommand]
        public void Duplicate(Guid? sessionId = null)
        {
            if (sessionId == null)
            {
                if (SelectedSession == null)
                {
                    Log.Warning($"There is no session to duplicate.", LoggingCategory);
                    return;
                }
                    

                sessionId = SelectedSession.ID;
            }

            var session = App.SessionManager.Sessions.FirstOrDefault(s => s.ID == sessionId);

            if (session is null)
            {
                Log.Warning($"Session {sessionId} not found.", LoggingCategory);
                return;
            }

            var newSession = App.SessionManager.Create(GetNextName(session.Name));
            newSession.IPAddress = session.IPAddress;
            newSession.Port = session.Port;
            newSession.PortInteractive = session.PortInteractive;
            newSession.UseHttps = session.UseHttps;
            newSession.ReconnectIterations = session.ReconnectIterations;
            newSession.KeepAlive = session.KeepAlive;
            newSession.AutoTimeout = session.AutoTimeout;

            Log.Information($"Duplicated session: {session.Name}", LoggingCategory);

            if (IsEditing)
                return;

            foreach (var s in Sessions)
            {
                s.IsSelected = s.ID == newSession.ID;
            }

            SelectedSession = newSession;
        }

        public string GetNextName(string baseName)
        {
            var names = Sessions.Select(s => s.Name).ToHashSet();

            // Remove an existing trailing number.
            // "Titan control 1" -> "Titan control"
            var match = Regex.Match(baseName, @"^(.*?)(?:\s+(\d+))?$");

            var rootName = match.Groups[1].Value.TrimEnd();

            // If the exact name doesn't exist, return it unchanged.
            if (!names.Contains(baseName))
                return baseName;

            int i = 1;

            while (names.Contains($"{rootName} {i}"))
                i++;

            return $"{rootName} {i}";
        }


        [RelayCommand]
        public async Task Remove(Guid? sessionId = null)
        {
            if (sessionId == null)
            {
                if (SelectedSession == null)
                {
                    Log.Warning($"There is no session to duplicate.", LoggingCategory);
                    return;
                }


                sessionId = SelectedSession.ID;
            }

            var session = App.SessionManager.Sessions.FirstOrDefault(s => s.ID == sessionId);

            if (session is null)
            {
                Log.Warning($"Session {sessionId} not found.", LoggingCategory);
                return;
            }

            var accepted = await MainWindow.DialogService
                        .ShowConfirmationAsync(
                            "Remove session",
                            $"Are you sure you wish to remove the session: {session.Name}.\nThis action cannot be undone.");

            if (!accepted)
                return;

            SelectedSession = Sessions.FirstOrDefault(s => s.ID == CurrentWorkspace.Options.Session);

            App.SessionManager.Remove((Guid)sessionId);

            Log.Information($"Removed session: {session.Name}", LoggingCategory);
        }
    }
}
