using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Disk.Converter;
using TitanControl.Disk.Interface;
using TitanControl.Logging;
using TitanControl.Session;
using TitanControl.WebAPI;

namespace TitanControl.Disk.Model.Session
{
    public class SessionModel : ISaveModel
    {
        [JsonPropertyName("sessionId")]
        public Guid ID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("ipAddress")]
        [JsonConverter(typeof(IPAddressArrayJsonConverter))]
        public IPAddress IPAddress { get; set; } = default!;

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
                IPAddress = IPAddress,
                Port = Port,
                PortInteractive = PortInteractive,
                UseHttps = UseHttps,
                ReconnectIterations = ReconnectIterations,
                KeepAlive = KeepAlive,
                AutoTimeout = AutoTimeout
            };
        }

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public Guid? OldId
        {
            set
            {
                if (value != null)
                    ID = (Guid)value;
            }
        }

        [JsonPropertyName("address")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public string? OldAddress
        {
            set
            {
                if (value != null && System.Net.IPAddress.TryParse(value, out IPAddress? address))
                    IPAddress = address;
                else
                    Log.Warning($"Could not parse old IPAddress string {value}, assuming default instead.", 
                        $"Session model[{ID}]");
            }
        }
    }
}
