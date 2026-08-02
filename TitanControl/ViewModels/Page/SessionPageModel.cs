using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Menu;
using TitanControl.Disk.Model.Session;
using TitanControl.Session.Utils;

namespace TitanControl.ViewModels
{
    public class SessionPageModel : BaseViewModel
    {
        public ObservableCollection<ScannedSession> Sessions { get; private set; } = new();
        private SessionScanner networkScanner;

        public SessionScanner NetworkScanner => networkScanner;

        public SessionPageModel()
        {
            networkScanner = new SessionScanner(
                IPAddress.Parse("192.168.2.100"),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(400),
                TimeSpan.FromSeconds(2),
                8,
                false
                ); 

            Sessions.Add(new ScannedSession
            {
                Name = "Example Session",
                ComputerName = "Example Computer",
                Address = IPAddress.Parse("127.0.0.1"),
                Port = 4430,
                PortInteractive = -1,
                State = Session.SessionConnectionState.Inactive,
            });

            Sessions.Add(new ScannedSession
            {
                Name = "Example Session 1",
                ComputerName = "Example Computer 1",
                Address = IPAddress.Parse("127.0.0.2"),
                Port = 4431,
                State = Session.SessionConnectionState.Available
            });

            Sessions.Add(new ScannedSession
            {
                Name = "Example Session 3",
                ComputerName = "Example Computer 3",
                Address = IPAddress.Parse("127.0.0.3"),
                Port = 4431,
                State = Session.SessionConnectionState.Connecting
            });

            Sessions.Add(new ScannedSession
            {
                Name = "Example Session 3",
                ComputerName = "Example Computer 3",
                Address = IPAddress.Parse("127.0.0.3"),
                Port = 4431,
                State = Session.SessionConnectionState.Connected
            });
            Sessions.Add(new ScannedSession
            {
                Name = "Example Session 3",
                ComputerName = "Example Computer 3",
                Address = IPAddress.Parse("127.0.0.3"),
                Port = 4431,
                State = Session.SessionConnectionState.Disconnected
            });
            Sessions.Add(new ScannedSession
            {
                Name = "Example Session 3",
                ComputerName = "Example Computer 3",
                Address = IPAddress.Parse("127.0.0.3"),
                Port = 4431,
                State = Session.SessionConnectionState.Unreachable
            });
        }

        public void OnLoaded()
        {
            _ = networkScanner.StartAsync();
        }

        public void SetList(SessionModel[] sessions)
        {

        }
    }
}
