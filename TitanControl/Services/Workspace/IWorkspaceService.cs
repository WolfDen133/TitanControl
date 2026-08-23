using System;
using System.Threading.Tasks;
using TitanControl.Events.Workspace;
using TitanControl.Models.Workspace;

namespace TitanControl.Services.Workspace
{
    public interface IWorkspaceService : IItemService<WorkspaceModel, Guid>
    {
        WorkspaceModel CurrentWorkspace { get; }
        bool HasWorkspace { get; } 
        bool HasLastWorkspace { get; }

        string[] WorkspaceNames { get; }

        event EventHandler<WorkspaceEventArgs>? WorkspaceCreated;
        event EventHandler<WorkspaceEventArgs>? WorkspaceDeleted;
        event EventHandler<WorkspaceEventArgs>? WorkspaceModified;
        event EventHandler<WorkspaceEventArgs>? WorkspaceSaved;
        event EventHandler<WorkspaceEventArgs>? WorkspacedLoaded;

        Task LoadAsync(Guid id);
        Task SaveAsync(WorkspaceModel workspace);
    }
}
