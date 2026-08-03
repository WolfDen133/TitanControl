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
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Session.Interface;
using TitanControl.Session.Utils;
using TitanControl.WebAPI;

namespace TitanControl.ViewModels
{
    public class SessionPageModel : BaseViewModel, INotifyPropertyChanged
    {
        private List<KeyValuePair<string, IPAddress>> _nics = new();
        private SessionScanner? _scanner;
        private int selectedNic;
        private bool refreshEnabled;
        private string animation = string.Empty;

        private ObservableCollection<ScannedSession> dummySessions { get; } = new()
        {
            new ScannedSession()
            {
                State = Session.SessionConnectionState.Inactive,
                PortInteractive = 4431
            },
            new ScannedSession()
            {
                State = Session.SessionConnectionState.Available
            },
            new ScannedSession()
            {
                State = Session.SessionConnectionState.Connected
            },
            new ScannedSession()
            {
                State = Session.SessionConnectionState.Connecting
            },
            new ScannedSession()
            {
                State = Session.SessionConnectionState.Disconnected
            },
            new ScannedSession()
            {
                State = Session.SessionConnectionState.Unreachable
            }
        };
        

        public ObservableCollection<string> Interfaces { get; private set; } = new();

        public ObservableCollection<SessionModel> Sessions { get; set; } = new();

        public ReadOnlyObservableCollection<ScannedSession> ScanResults =>
            _scanner == null ? new(dummySessions) : Scanner?.Results!;

        public SessionScanner? Scanner
        {
            get => _scanner;
            private set
            {
                if (ReferenceEquals(_scanner, value))
                    return;

                _scanner = value;

                OnPropertyChanged();
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

        public string Animation
        {
            get => animation;
            set
            {
                animation = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Animation));
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

        public async Task RegisterScanner(bool start = false)
        {
            if (Scanner != null)
            {
                await Scanner.DisposeAsync();
                Scanner = null;
            }

            Scanner = new SessionScanner(
                _nics[SelectedNic].Value,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromSeconds(2),
                12,
                false
            );

            Scanner.OnScannerRunning += (_, isRunning) =>
            {
                RefreshEnabled = !isRunning;
                Animation = isRunning ? "running" : string.Empty;
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

        public void SetList(SessionModel[] sessions)
        {

        }
    }
}
