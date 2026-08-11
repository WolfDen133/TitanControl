using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.Disk.Interface;
using TitanControl.WebAPI.Data;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceControlModel : ISaveModel
    {
        public ControlId ControlId { get; set; }
        public int[] Location { get; set; } = new int[4];
        public int TitanId { get; set; }
        public HandleType HandleType { get; set; }
        public KeyProfile KeyProfile { get; set; }

        public ISaveable ToInstance()
        {
            return new BaseHandleControl
            {
                ControlId = ControlId,
                Location = FileUtilities.ToRectangleFromArray(Location),
                TitanId = TitanId,
                HandleType = HandleType,
                KeyProfile = KeyProfile
            };
        }
    }
}
