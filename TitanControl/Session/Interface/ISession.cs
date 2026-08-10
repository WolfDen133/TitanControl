using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Session.Event;
using TitanControl.WebAPI;

namespace TitanControl.Session.Interface
{
    public interface ISession : IDisposable, INotifyPropertyChanged
    {
        [JsonPropertyName("sessionId")]
        Guid ID { get; }

        [JsonPropertyName("name")]
        string Name { get; set; }

        [JsonPropertyName("ipAddress")]
        IPAddress IPAddress { get; set; }

        [JsonPropertyName("port")]
        int Port { get; set; }

        [JsonPropertyName("portInteractive")]
        int? PortInteractive { get; set; }

        [JsonPropertyName("useHttps")]
        bool UseHttps { get; set; }

        [JsonPropertyName("reconnectIterations")]
        int ReconnectIterations { get; set; }

        [JsonPropertyName("keepAlive")]
        int KeepAlive { get; set; }

        [JsonPropertyName("autoTimeout")]
        int? AutoTimeout { get; set; }

        [JsonIgnore]
        string ComputerName { get; set; }

        [JsonIgnore]
        Titan? Api { get; }

        [JsonIgnore]
        SessionConnectionState State { get; set; }

        [JsonIgnore]
        bool IsConnected { get; }

        [JsonIgnore]
        DateTimeOffset? LastSuccessfulKeepAlive { get; }

        [JsonIgnore]
        DateTime? ConnectedAt { get; }

        [JsonIgnore]
        bool IsSelected { get; set; }

        void Start(IPAddress selectedInterface);
        void Stop();

        void Enable(bool enabled = true);

        event EventHandler<SessionStateChangedEventArgs>? StateChanged;
    }
}
