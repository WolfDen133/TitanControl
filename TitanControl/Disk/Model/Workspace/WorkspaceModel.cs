using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.Disk.Interface;
using TitanControl.Workspaces;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceModel : ISaveModel
    {
        [JsonPropertyName("workspaceId")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public int Version { get; } = 1;

        [JsonPropertyName("options")]
        public WorkspaceOptionsModel Options { get; set; } = null!;

        [JsonPropertyName("controls")]
        public List<WorkspaceControlModel> Controls { get; set; } = null!;

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; }

        [JsonPropertyName("settings")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public WorkspaceOptionsModel? LegacySettings
        {
            set
            {
                if (value != null)
                    Options = value;
            }
        }

        [JsonPropertyName("uiElements")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public List<WorkspaceControlModel>? LegacyControls
        {
            set
            {
                if (value != null)
                    Controls = value;
            }
        }

        public ISaveable ToInstance()
        {
            return new Workspaces.Workspace
            {
                Id = Id,
                Name = Name,
                Options = (WorkspaceOptions)Options.ToInstance(),
                Controls = Controls.Select(c => (BaseHandleControl)c.ToInstance()).ToList(),
                LastModified = LastModified
            };
        }
    }
}
