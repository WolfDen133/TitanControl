using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;

namespace TitanControl.Controls.Handle
{
    public interface ITitanControl
    {
        public ControlId ControlId { get; set; }
        public Rectangle Location { get; set; }
        public int TitanId { get; set; }
        public HandleType HandleType { get; set; }
        public KeyProfile KeyProfile { get; set; }
    }
}
