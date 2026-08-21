using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk.Resporitory;
using TitanControl.Models.Workspace;

namespace TitanControl.Disk.Resporitory.Workspace
{
    public interface IWorkspaceRepository : IRepository<WorkspaceModel>
    {
        Guid LastWorkspace { get; }
        bool HasLastWorkspace => LastWorkspace != Guid.Empty;

        Task<WorkspaceModel> LoadAsync(Guid id);
    }
}
