using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Session;

namespace TitanControl.Disk.Model.Session
{
    public class SessionModel
    {
        [JsonPropertyName("sessionId")]
        public Guid ID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("ipAddress")]
        public IPAddress IPAddress { get; set; } = new IPAddress([127, 0, 0, 1]);

        [JsonPropertyName("port")]
        public int Port { get; set; } = 4430;

        [JsonPropertyName("portInteractive")]
        public int? PortInteractive { get; set; } = null;

        [JsonPropertyName("useHttps")]
        public bool UseHttps { get; set; } = false;

        [JsonPropertyName("reconnectIterations")]
        public int ReconnectIterations { get; set; } = 5;

        [JsonPropertyName("keepAlive")]
        public int KeepAlive { get; set; } = 5;

        [JsonPropertyName("autoTimeout")]
        public int? AutoTimeout { get; set; } = null;


        [JsonIgnore]
        public string ComputerName { get; set; } = string.Empty;

        [JsonIgnore]
        public SessionConnectionState State { get; set; } = SessionConnectionState.Disabled;

        [JsonIgnore]
        public DateTime? ConnectedAt { get; set; } = DateTime.Now.AddHours(-1);

        [JsonIgnore]
        public bool IsSelected { get; set; } = false;


    }
}
