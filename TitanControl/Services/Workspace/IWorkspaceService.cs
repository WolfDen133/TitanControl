using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Events.Workspace;
using TitanControl.Models.Workspace;

namespace TitanControl.Services.Workspace
{
    public interface IWorkspaceService : IItemService<WorkspaceModel, Guid>
    {
        WorkspaceModel CurrentWorkspace { get; }

        bool HasCurrentWorkspace { get; }

        event EventHandler<WorkspaceEventArgs>? WorkspaceCreated;
        event EventHandler<WorkspaceEventArgs>? WorkspaceDeleted;
        event EventHandler<WorkspaceEventArgs>? WorkspaceModified;
        event EventHandler<WorkspaceEventArgs>? WorkspaceSaved;
        event EventHandler<WorkspaceEventArgs>? WorkspacedLoaded;
    }
}
