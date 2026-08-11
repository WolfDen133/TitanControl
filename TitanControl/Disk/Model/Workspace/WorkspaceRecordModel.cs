using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Disk.Interface;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceRecordModel : ISaveModel
    {
        [JsonPropertyName("lastWorkspace")]
        public Guid LastWorkspace { get; set; } = Guid.Empty;

        [JsonPropertyName("workspaces")]
        public Dictionary<Guid, WorkspaceEntryModel> Workspaces { get; set; } = new();
    }
}
