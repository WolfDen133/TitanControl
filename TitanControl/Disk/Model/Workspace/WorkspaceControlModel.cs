using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.Disk.Converter;
using TitanControl.Disk.Interface;
using TitanControl.WebAPI.Data;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceControlModel : ISaveModel
    {
        public ControlId ControlId { get; set; }

        [JsonConverter(typeof(RectangleArrayJsonConverter))]
        public Rectangle Location { get; set; } 
        public int TitanId { get; set; }
        [JsonIgnore]
        public HandleType HandleType { get; set; } = HandleType.None;
        public KeyProfile KeyProfile { get; set; }

        [JsonPropertyName("handleType")]
        public HandleType? HandleTypeJson
        {
            get => HandleType;
            set => HandleType = value ?? HandleType.None;
        }


        public ISaveable ToInstance()
        {
            return new BaseHandleControl
            {
                ControlId = ControlId,
                Location = Location,
                TitanId = TitanId,
                HandleType = HandleType,
                KeyProfile = KeyProfile
            };
        }
    }
}
