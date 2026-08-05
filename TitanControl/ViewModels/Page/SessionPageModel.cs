using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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

        public SessionFormModel FormData { get; set; } = new();

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

        public async Task UpdateSessions()
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
                };

                Sessions.Add(model);
            }
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

        private void UpdateFormData()
        {
            if (SelectedSession is null)
            {
                var ex = new InvalidOperationException("Selected session is null.");
                Log.Error(ex, "Selected session is invalid", LoggingCategory);
                return;
            }

            FormData = new()
            {
                SessionName = SelectedSession.Name,
                IpAddress = SelectedSession.IPAddress.ToString(),
                Port = SelectedSession.Port,
                PortInteractive = SelectedSession.PortInteractive,
                AutoTimeout = SelectedSession.AutoTimeout != null,
                Reconnect = SelectedSession.ReconnectIterations != 0,
                AutoTimeoutMinuates = SelectedSession.AutoTimeout,
                ReconnectAttempts = SelectedSession.ReconnectIterations,
                KeepAliveSeconds = SelectedSession.KeepAlive,
                UseHttps = SelectedSession.UseHttps
            };

            OnPropertyChanged(nameof(FormData));

            Log.Debug("Updated Form Data");
        }

        [RelayCommand]
        public void SessionQuickAction (Guid sessionId)
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

            switch (session.State) {
                case SessionConnectionState.Available:
                    break;
                    
            }
        }

        [RelayCommand]
        public void AddManual()
        {
            Log.Debug($"Hit add manual command");
        }

        [RelayCommand]
        public void SaveSession()
        {
            Log.Debug($"Session form results: {FormData.SessionName}, {FormData.IpAddress}, {FormData.Port}, {FormData.PortInteractive}, {FormData.AutoTimeout}, {FormData.Reconnect}, {FormData.AutoTimeoutMinuates}, {FormData.KeepAliveSeconds}, {FormData.ReconnectAttempts}, {FormData.UseHttps}");
        }

        [RelayCommand]
        public void Cancel()
        {
            Log.Debug($"Hit cancel command");
        }


        [RelayCommand]
        public void Connect()
        {
            Log.Debug($"Hit connect command");
        }


        [RelayCommand]
        public void Disconnect()
        {
            Log.Debug($"Hit disconnect command");
        }


        [RelayCommand]
        public void Edit()
        {
            Log.Debug($"Hit edit command");
            UpdateFormData();
        }


        [RelayCommand]
        public void Duplicate()
        {
            Log.Debug($"Hit duplicate command");
        }


        [RelayCommand]
        public void Remove()
        {
            Log.Debug($"Hit remove command");
        }
    }
}
