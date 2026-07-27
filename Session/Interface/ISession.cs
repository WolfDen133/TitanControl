using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanWebAPI;

namespace TitanControl.Session.Interface
{
    public interface ISession<Titan> : IDisposable
    {
        UUID ID { get; }
        string Name { get; }
        Titan Api { get; }
        SessionConnectionState State { get; }
        bool IsConnected { get; }
        DateTimeOffset? LastSucessfulKeepAlive { get; }

        void Start();
        void Stop();

        event EventHandler<SessionStateChangedEventArgs> StateChanged;

    }
}
