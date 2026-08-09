using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Menu;
using TitanControl.Disk.Model.Session;
using TitanControl.Disk.Model.Workspace;
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
        private SessionScanner? _scanner;
        private int selectedNic;
        private bool refreshEnabled;
        private ObservableCollection<SessionModel> sessions = Design.IsDesignMode ? new(dummySessions) : new();
        public bool editing = false;
        private SessionFormModel? formModel;

        private SessionModel? selectedSession = new SessionModel
        {
            ID = Guid.NewGuid(),
            Name = "Test session"
        };

        private static ObservableCollection<SessionModel> dummySessions { get; } = new()
        {
            new SessionModel()
            {
                Name = "Titan Session",
                State = Session.SessionConnectionState.Disabled,
                PortInteractive = 4431,
                ComputerName = "Test computer"
            },
            new SessionModel()
            {
                Name = "Titan Session",
                State = Session.SessionConnectionState.Enabled,
                PortInteractive = 4431
            },
            new SessionModel()
            {
                Name = "Titan Session",
                State = Session.SessionConnectionState.Available
            },
            new SessionModel()
            {
                Name = "Titan Session",
                State = Session.SessionConnectionState.Connected
            },
            new SessionModel()
            {
                Name = "Titan Session",
                State = Session.SessionConnectionState.Connecting
            },
            new SessionModel()
            {
                Name = "Titan Session",
                State = Session.SessionConnectionState.Disconnected
            },
            new SessionModel()
            {
                Name = "Titan Session",
                State = Session.SessionConnectionState.Unreachable
            }
        };

        public ObservableCollection<string> Interfaces { get; private set; } = new();

        public ObservableCollection<SessionModel> Sessions 
        { 
            get => sessions;
            set
            {
                sessions = value;
                OnPropertyChanged(nameof(EnabledSessions));
            }
        }

        public SessionModel? SelectedSession 
        { 
            get => selectedSession; 
            set
            {
                selectedSession = value;
                OnPropertyChanged(nameof(SelectedSession));
                OnPropertyChanged(nameof(Endpoint));
            }
        }

        public SessionScanner? Scanner
        {
            get => _scanner;
            private set
            {
                if (ReferenceEquals(_scanner, value))
                    return;

                _scanner = value;

                OnPropertyChanged(nameof(ScanResults));
                OnPropertyChanged(nameof(RefreshEnabled));
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

        public ObservableCollection<SessionModel> EnabledSessions => 
            Design.IsDesignMode 
                ? new ObservableCollection<SessionModel>
                {
                    new SessionModel
                    {
                        ID = Guid.Empty,
                        State = Session.SessionConnectionState.Enabled,
                        Name = "Enabled session"
                    },
                    new SessionModel
                    {
                        ID = Guid.Empty,
                        State = Session.SessionConnectionState.Connected,
                        Name = "Active session"
                    }
                }
                : new ObservableCollection<SessionModel>(sessions.Where(
                    s => s.State is
                        Session.SessionConnectionState.Enabled or
                        Session.SessionConnectionState.Disabled or
                        Session.SessionConnectionState.Connected or
                        Session.SessionConnectionState.Connected).ToArray());

        public ReadOnlyObservableCollection<SessionModel> ScanResults => Scanner?.Results!;

        public string Endpoint =>
         SelectedSession is not null
         ? (SelectedSession?.PortInteractive is not null
             ? $"{SelectedSession!.IPAddress} : {SelectedSession!.Port}/{SelectedSession!.PortInteractive!}"
             : $"{SelectedSession!.IPAddress} : {SelectedSession!.Port}")
         : "-";

        public WorkspaceModel CurrentWorkspace => App.WorkspaceManager.CurrentWorkspace;

        public SessionPageModel? CurrentSession { get; set; }

        public async Task RegisterScanner(bool start = false)
        {
            if (Scanner != null)
            {
                await Scanner.DisposeAsync();
                Scanner = null;
            }

            Scanner = new SessionScanner(
                _nics[SelectedNic].Value,
                TimeSpan.FromSeconds(20),
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromSeconds(4),
                24,
                false
            );

            Scanner.OnScannerRunning += (_, isRunning) =>
            {
                RefreshEnabled = !isRunning;
            };

            if (start)
                await StartScanner();
        }

        public void Initialize()
        {
            var nics = NicHelper.GetNics();
            var defaultIp = NicHelper.GetDefaultIPv4Address();

            foreach (var nic in nics)
            {
                var nicRecord = new KeyValuePair<string, IPAddress>(nic.Key, nic.Value);
                Interfaces.Add($"{nic.Key} - {nic.Value}");
                _nics.Add(nicRecord);

                if (nicRecord.Value.Equals(defaultIp) && Scanner is null)
                {
                    selectedNic = _nics.IndexOf(nicRecord);
                    OnPropertyChanged(nameof(SelectedNic));

                    _ = RegisterScanner();

                    Log.Debug($"Default nic {defaultIp}, assigned to index {SelectedNic}");
                }   
            }
        }

        public void UpdateSessions()
        {
            foreach(var session in App.SessionManager.GetAll())
            {
                var model = new SessionModel
                {
                    ID = session.ID,
                    Name = session.Name,
                    ComputerName = session.Api.ConnectedDevice?.ComputerName ?? "-",
                    IPAddress = session.Api.Address,
                    Port = session.Api.Port,
                    PortInteractive = session.Api.PortInteractive,
                    State = session.State == SessionConnectionState.Available 
                         && CurrentWorkspace.Options.Session == session.ID 
                        ? SessionConnectionState.Enabled 
                        : SessionConnectionState.Disabled
                };

                Sessions.Add(model);
            }

            editing = true;
        }

        public async Task StartScanner()
        {
            await Scanner?.StartAsync()!;
        }

        public void StopScanner()
        {
            Scanner?.Stop();
        }

        public void SelectSession(Guid id)
        {
            var session = Sessions.FirstOrDefault(s => s.ID == id);

            if (session == null)
            {
                var ex = new InvalidOperationException("Session was not found.");
                Log.Error(ex, "Selected session was not found in list", LoggingCategory);
                return;
            }

            SelectedSession = session;
        }

        public async Task Clear()
        {
            Sessions.Clear();
            await Scanner?.ClearResultsAsync()!;
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
            FormData = null;
            IsEditing = false;

            OnPropertyChanged(nameof(FormData));
        }

        [RelayCommand]
        public void SessionQuickAction(Guid sessionId)
        {
            Log.Debug($"Hit quick action command for {sessionId}");

            SessionModel? session = Sessions.FirstOrDefault(s => s.ID == sessionId);
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
                    // TODO
                    break;
                case SessionConnectionState.Disconnected:
                    Connect();
                    break;
                case SessionConnectionState.Connected:
                    Disconnect();
                    break;
                case SessionConnectionState.Disabled:
                    CurrentWorkspace.Options.Session = sessionId;
                    SelectedSession!.State = SessionConnectionState.Enabled;
                    OnPropertyChanged(nameof(SelectedSession));
                    break;
                case SessionConnectionState.Enabled:
                    Connect();

                    break;
                case SessionConnectionState.Unreachable:
                    // TODO
                    break;
            }
        }

        [RelayCommand]
        public void AddManual()
        {
            Log.Debug($"Hit add manual command");

            int sessionIndex = Sessions.Count;
            string sessionName = string.Empty;
            do {
                sessionIndex++;
                sessionName = $"Titan Session {sessionName}";
            } while (Sessions.FirstOrDefault(s => s.Name == sessionName) is SessionModel);

            SelectedSession = new SessionModel()
            {
                ID = Guid.NewGuid(),
                Name = sessionName
            };

            EnableForm();
        }

        [RelayCommand]
        public void SaveSession()
        {
            SelectedSession = FormData.ToModel();
            App.SessionManager.Update(SelectedSession.ID, SelectedSession);

            DisableForm();
        }

        [RelayCommand]
        public async Task Cancel()
        {
            Log.Debug($"Hit cancel command");

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

            if (!App.SessionManager.TryGet((Guid)sessionId, out var session))
            {
                Log.Warning($"Session {sessionId} not found.");
                return;
            }

            session?.Start();
            
            Log.Debug($"Hit connect command");
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

            if (!App.SessionManager.TryGet((Guid)sessionId, out var session))
            {
                Log.Warning($"Session {sessionId} not found.");
                return;
            }

            session?.Stop();
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
            Log.Debug($"Hit duplicate command");

            if (sessionId == null)
            {
                if (SelectedSession == null)
                    return;

                sessionId = SelectedSession.ID;
            }

            if (!App.SessionManager.TryGet((Guid)sessionId, out var session))
            {
                Log.Warning($"Session {sessionId} not found.");
                return;
            }

            int identicalSessions = App.SessionManager
                .GetAll()
                .Where(s => s.Name == session!.Name)
                .Count();

            identicalSessions++;

            
        }


        [RelayCommand]
        public void Remove()
        {
            Log.Debug($"Hit remove command");
        }
    }
}
