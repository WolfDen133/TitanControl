using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Events.Session;
using TitanControl.WebAPI;

namespace TitanControl.Services.Session
{
    public interface ISession : IDisposable, INotifyPropertyChanged
    {
        Guid ID { get; }
        string Name { get; set; }
        IPAddress IPAddress { get; set; }
        int Port { get; set; }
        int? PortInteractive { get; set; }
        bool UseHttps { get; set; }
        int ReconnectIterations { get; set; }
        int KeepAlive { get; set; }
        int? AutoTimeout { get; set; }
        string ComputerName { get; set; }
        Titan? Api { get; }
        SessionConnectionState State { get; set; }
        bool IsConnected { get; }
        DateTimeOffset? LastSuccessfulKeepAlive { get; }
        DateTime? ConnectedAt { get; }
        bool IsSelected { get; set; }

        void Start(IPAddress selectedInterface);
        void Stop();
        void Enable(bool enabled = true);

        event EventHandler<SessionStateChangedEventArgs>? StateChanged;
    }
}
