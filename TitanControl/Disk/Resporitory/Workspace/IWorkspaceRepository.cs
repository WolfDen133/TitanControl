using System;
using System.Threading.Tasks;
using TitanControl.Models.Workspace;

namespace TitanControl.Disk.Resporitory.Workspace
{
    public interface IWorkspaceRepository : IRepository<WorkspaceModel>
    {
        Guid LastWorkspace { get; }

        Task<WorkspaceModel> LoadAsync(Guid id);
        Task LoadRecord();

        string[] WorkspaceNames { get; }
    }
}
