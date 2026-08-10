using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Menu;
using TitanControl.Session.Interface;

namespace TitanControl.Events.Control
{
    public class SessionOverviewSelectedEventArgs : EventArgs
    {
        public bool IsCanceled { get; set; } = false;
        public Guid SessionId { get; }

        public bool IsSelected { get; }

        public SessionOverviewSelectedEventArgs(Guid sessionId, bool isSelected)
        {
            SessionId = sessionId;
            IsSelected = isSelected;
        }
    }
}
