using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using TitanControl.Events.Session;
using TitanControl.WebAPI;

namespace TitanControl.Services.Session
{
    public interface ISession : IAsyncDisposable, INotifyPropertyChanged
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

        Task Start(IPAddress selectedInterface);
        Task Stop();
        void Enable();
        void Disable();

        event EventHandler<SessionStateChangedEventArgs>? StateChanged;
    }
}
