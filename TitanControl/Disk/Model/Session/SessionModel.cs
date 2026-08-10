using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Session;
using TitanControl.Session.Event;
using TitanControl.Session.Interface;
using TitanControl.WebAPI;

namespace TitanControl.Disk.Model.Session
{
    public class SessionModel : ISession
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public IPAddress IPAddress { get; set; } = new IPAddress([127, 0, 0, 1]);
        public int Port { get; set; } = 4430;
        public int? PortInteractive { get; set; } = null;
        public bool UseHttps { get; set; } = false;
        public int ReconnectIterations { get; set; } = 5;
        public int KeepAlive { get; set; } = 5;
        public int? AutoTimeout { get; set; } = null;

        public string ComputerName { get; set; } = string.Empty;
        public SessionConnectionState State { get; set; } = SessionConnectionState.Disabled;
        public DateTime? ConnectedAt { get; set; } = DateTime.Now.AddHours(-1);
        public bool IsSelected { get; set; } = false;
        public Titan? Api => throw new NotImplementedException();
        public bool IsConnected => throw new NotImplementedException();
        public DateTimeOffset? LastSuccessfulKeepAlive => throw new NotImplementedException();

        public event EventHandler<SessionStateChangedEventArgs>? StateChanged = null;
        public event PropertyChangedEventHandler? PropertyChanged = null;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Enable(bool enabled = true)
        {
            throw new NotImplementedException();
        }

        public void Start(IPAddress selectedInterface)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
