using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.ViewModels.Workspace;

namespace TitanControl.Events.Workspace
{
    public class WorkspaceActionEventArgs : RoutedEventArgs
    {
        public WorkspaceActionEventArgs(RoutedEvent routedEvent, WorkspaceAction action) : base(routedEvent)
        {
            Action = action;
        }

        public WorkspaceAction Action { get; }
    }
}
