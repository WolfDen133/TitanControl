using System;
using System.Threading.Tasks;
using TitanControl.Models.Workspace;

namespace TitanControl.Disk.Resporitory.Workspace
{
    public interface IWorkspaceRepository : IRepository<WorkspaceModel>
    {
        Guid LastWorkspace { get; }

        Task<WorkspaceModel> LoadAsync(Guid id);
        Task<WorkspaceModel> TryLoadAsync(string path);

        Task<string?> SaveAsync(WorkspaceModel workspace, string? path);
        Task<string?> RenameAsync(WorkspaceModel workspace);

        Task LoadRecord();

        string[] WorkspaceNames { get; }
    }
}
