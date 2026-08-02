using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;

namespace TitanControl.Controls.Handle
{
    public class BaseHandleControl : Control, ITitanControl
    {
        public ControlId ControlId { get; set; } = ControlId.None;
        public Rectangle Location { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int TitanId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public HandleType HandleType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public KeyProfile KeyProfile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
