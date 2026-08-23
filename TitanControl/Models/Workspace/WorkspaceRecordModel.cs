using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TitanControl.Models.Workspace
{
    public class WorkspaceRecordModel : ISaveModel
    {
        [JsonPropertyName("lastWorkspace")]
        public Guid LastWorkspace { get; set; } = Guid.Empty;

        [JsonPropertyName("workspaces")]
        public Dictionary<Guid, WorkspaceEntryModel> Workspaces { get; set; } = new();

        [JsonPropertyName("workspaceEntries")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public Dictionary<Guid, WorkspaceEntryModel>? LegacyWorkspaceEntries
        {
            set
            {
                if (value != null)
                    Workspaces = value;
            }
        }
    }
}
