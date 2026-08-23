using System;

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
