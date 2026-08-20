using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk;
using TitanControl.Disk.Interface;
using TitanControl.Disk.Model.Workspace;
using TitanControl.WebAPI.Data;

namespace TitanControl.Controls.Handle
{
    public class BaseHandleControl : Control, ITitanControl, ISaveable
    {
        public ControlId ControlId { get; set; } = ControlId.None;
        public Rectangle Location { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int TitanId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public HandleType HandleType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public KeyProfile KeyProfile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
