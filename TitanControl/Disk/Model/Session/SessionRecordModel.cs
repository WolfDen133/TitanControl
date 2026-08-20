using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Disk.Interface;

namespace TitanControl.Disk.Model.Session
{
    public class SessionRecordModel : ISaveModel
    {
        [JsonPropertyName("lastSession")]
        public Guid LastSession { get; set; } = Guid.Empty;

        [JsonPropertyName("sessions")]
        public List<SessionModel> Sessions { get; set; } = [];
    }
}
