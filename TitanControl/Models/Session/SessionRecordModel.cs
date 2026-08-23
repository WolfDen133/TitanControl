using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using TitanControl.Disk.Converter;

namespace TitanControl.Models.Session
{
    public class SessionRecordModel : ISaveModel
    {
        [JsonPropertyName("localInterface")]
        [JsonConverter(typeof(IPAddressArrayJsonConverter))]
        public IPAddress? LocalInterface { get; set; }

        [JsonPropertyName("scanDuration")]
        public int ScannerDuration { get; set; } = 10;

        [JsonPropertyName("scanTimeout")]
        public int ScannerTimeout { get; set; } = 300;

        [JsonPropertyName("scanPhaseDelay")]
        public int ScannerDelay { get; set; } = 4;

        [JsonPropertyName("scanThreads")]
        public int ScannerConcurrency { get; set; } = 12;

        [JsonPropertyName("scanHttps")]
        public bool ScannerUseHttps { get; set; } = false;

        [JsonPropertyName("sessions")]
        public List<SessionSaveModel> Sessions { get; set; } = [];
    }
}
