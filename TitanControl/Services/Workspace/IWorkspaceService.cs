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
        event EventHandler<WorkspaceEventArgs>? WorkspaceSaved;
        event EventHandler<WorkspaceEventArgs>? WorkspacedLoaded;

        Task LoadAsync(Guid id);
        Task LoadAsync(string path);
        Task<string?> RenameAsync(WorkspaceModel model);
        Task<string?> SaveAsync(WorkspaceModel workspace, string? path);
        new Task<string?> SaveAsync();

    }
}
