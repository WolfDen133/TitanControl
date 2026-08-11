using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Disk.Interface;
using TitanControl.Session.Interface;
using TitanControl.WebAPI;
using TitanControl.Workspaces;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceOptionsModel : ISaveModel
    {
        [JsonPropertyName("session")]
        public Guid? Session { get; set; }

        [JsonPropertyName("gridSize")]
        public int[] GridSize { get; set; } = new int[2];

        public ISaveable ToInstance()
        {
            return new WorkspaceOptions
            {
                Session = Session,
                GridSize = FileUtilities.ToSizeFromArray(GridSize)
            };
        }
    }
}
