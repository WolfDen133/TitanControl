using System;
using TitanControl.Models.Workspace;

namespace TitanControl.Events.Workspace
{
    public class WorkspaceEventArgs : EventArgs
    {
        public WorkspaceModel Workspace { get; }

        public WorkspaceEventArgs(WorkspaceModel workspace)
        {
            Workspace = workspace;
        }
    }
}
