using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Models.Workspace;

namespace TitanControl.Disk.Resporitory.Workspace
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private const string LoggingCategory = "Workspace Repo";

        private WorkspaceRecordModel _record;
        private FileHandler _fileHandler;

        public WorkspaceRepository(WorkspaceRecordModel record, FileHandler fileHandler)
        {
            _record = record;
            _fileHandler = fileHandler;
        }

        public async Task<WorkspaceModel> LoadAsync(Guid id)
        {
            // Existance check
            if (!_record.Workspaces.TryGetValue(id, out WorkspaceEntryModel? entryModel))
            {
                var ex = new InvalidOperationException("Workspace could not be found");
                Log.Error(ex, $"Workspace with key {id} could not be found in record.", LoggingCategory);
                throw ex;
            }

            // Locate workspace path by id
            string path;
            if (_record.Workspaces[id].Path != string.Empty)
                path = _record.Workspaces[id].Path;
            else
                path = Path.Combine(PathHelper.DocumentsPath, entryModel.Name + ".tcw");

            // Load async
            return await LoadByPathAsync(path);
        }

        public async Task<WorkspaceModel> LoadByPathAsync(string path)
        {
            // Load
            var newWorkspace = await _fileHandler.LoadWorkspace(path);

            // Open
            _record.LastWorkspace = newWorkspace.Id;

            // Save the entry
            await _fileHandler.SaveWorkspaceRecord(_record);

            Log.Information($"Loaded {newWorkspace.Id}: {path}", LoggingCategory);

            return newWorkspace;
        }

        public async Task SaveAsync(WorkspaceModel workspace)
        {
            // Get or create entry
            if (!_record.Workspaces.TryGetValue(workspace.Id, out WorkspaceEntryModel? entryModel))
            {
                entryModel = new WorkspaceEntryModel
                {
                    Name = workspace.Name,
                    Path = Path.Combine(PathHelper.DocumentsPath, workspace.Name + ".tcw"),
                };

                _record.Workspaces.Add(workspace.Id, entryModel);
            }

            // Save
            await _fileHandler.SaveWorkspace(workspace, entryModel.Path);
            await _fileHandler.SaveWorkspaceRecord(_record);

            Log.Information($"Saved {workspace.Id} to: {entryModel.Path}", LoggingCategory);
        }

        public void Dispose()
        {
            _record = null!;
        }
    }
}
