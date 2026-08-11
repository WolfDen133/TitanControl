using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.WebAPI;
using TitanControl.Session;
using TitanControl.Disk.Interface;

namespace TitanControl.Disk.Model
{
    public class SessionModel : ISaveModel
    {
        [JsonPropertyName("sessionId")]
        public Guid ID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("ipAddress")]
        public string IPAddress { get; set; } = string.Empty;
        [JsonPropertyName("port")]
        public int Port { get; set; } = 4430;
        [JsonPropertyName("portInteractive")]
        public int? PortInteractive { get; set; } = null;
        [JsonPropertyName("useHttp")]
        public bool UseHttps { get; set; } = false;
        [JsonPropertyName("reconnectAttempts")]
        public int ReconnectIterations { get; set; } = 5;
        [JsonPropertyName("keepAliveInterval")]
        public int KeepAlive { get; set; } = 5;
        [JsonPropertyName("autoTimeout")]
        public int? AutoTimeout { get; set; } = null;

        public ISaveable ToInstance()
        {
            return new TitanSession(ID, Name)
            {
                IPAddress = System.Net.IPAddress.Parse(IPAddress),
                Port = Port,
                PortInteractive = PortInteractive,
                UseHttps = UseHttps,
                ReconnectIterations = ReconnectIterations,
                KeepAlive = KeepAlive,
                AutoTimeout = AutoTimeout
            };
        }
    }
}
