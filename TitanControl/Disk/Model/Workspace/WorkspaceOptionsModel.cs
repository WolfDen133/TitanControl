using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Disk.Converter;
using TitanControl.Disk.Interface;
using TitanControl.Session.Interface;
using TitanControl.WebAPI;
using TitanControl.Workspaces;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceOptionsModel : ISaveModel
    {
        [JsonPropertyName("session")]
        public Guid Session { get; set; } = Guid.NewGuid();

        [JsonPropertyName("gridSize")]
        [JsonConverter(typeof(SizeArrayJsonConverter))]
        public Size GridSize { get; set; } = new Size(18, 18);

        public ISaveable ToInstance()
        {
            return new WorkspaceOptions
            {
                Session = Session,
                GridSize = GridSize
            };
        }
    }
}
