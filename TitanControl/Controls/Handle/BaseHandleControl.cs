using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk;
using TitanControl.Disk.Model.Workspace;
using TitanControl.Models;
using TitanControl.WebAPI.Data;

namespace TitanControl.Controls.Handle
{
    public class BaseHandleControl : Control, ITitanControl, ISaveable
    {

        public ISaveModel ToModel()
        {
            return new WorkspaceControlModel
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
