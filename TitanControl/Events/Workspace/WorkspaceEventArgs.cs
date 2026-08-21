using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Models.Workspace;

namespace TitanControl.Events.Workspace
{
    public class WorkspaceEventArgs : EventArgs
    {
        public WorkspaceModel Workspace { get; }

        public WorkspaceEventArgs (WorkspaceModel workspace)
        {
            Workspace = workspace;
        }
    }
}
