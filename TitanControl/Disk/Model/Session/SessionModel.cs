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
        public int PortInteractive { get; set; } = -1;

        [JsonPropertyName("useHttps")]
        public bool UseHttps = false;

        [JsonIgnore]
        public string ComputerName { get; set; } = string.Empty;

        [JsonIgnore]
        public SessionConnectionState State { get; set; } = SessionConnectionState.Inactive;
    }
}
