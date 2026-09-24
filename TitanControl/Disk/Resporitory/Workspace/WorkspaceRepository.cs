using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Models.Workspace;

namespace TitanControl.Disk.Resporitory.Workspace
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private const string LoggingCategory = "Workspace Repo";

        private FileHandler _fileHandler;
        private WorkspaceRecordModel _record = null!;

        public Guid LastWorkspace => _record.LastWorkspace;

        public string[] WorkspaceNames => [.. _record.Workspaces.Values.Select(x => x.Name)];

        public WorkspaceRepository(FileHandler fileHandler)
        {
            _fileHandler = fileHandler;
        }

        public async Task LoadRecord()
        {
            _record = await _fileHandler.LoadWorkspaceRecord();

            foreach (var entry in _record.Workspaces)
            {
                if (!File.Exists(entry.Value.Path))
                    _record.Workspaces.Remove(entry.Key);
            }

            if (!_record.Workspaces.ContainsKey(LastWorkspace))
                _record.LastWorkspace = Guid.Empty;
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

        public async Task<WorkspaceModel> TryLoadAsync(string path)
        {
            var workspace = await LoadByPathAsync(path, false);

            _record.LastWorkspace = workspace.Id;

            if (!_record.Workspaces.ContainsKey(workspace.Id))
            {
                _record.Workspaces.Add(workspace.Id, new WorkspaceEntryModel
                {
                    Name = workspace.Name,
                    Path = path,
                });
            }

                await _fileHandler.SaveWorkspaceRecord(_record);

            Log.Information($"Loaded workspace '{workspace.Name}': {path}", LoggingCategory);
            return workspace;
        }

        public async Task<WorkspaceModel> LoadByPathAsync(string path, bool save = true)
        {
            // Load
            var newWorkspace = await _fileHandler.LoadWorkspace(path);

            // Open
            _record.LastWorkspace = newWorkspace.Id;

            // Save the entry
            if (save)
                await _fileHandler.SaveWorkspaceRecord(_record);

            Log.Information($"Loaded workspace '{newWorkspace.Name}': {path}", LoggingCategory);

            return newWorkspace;
        }

        public async Task<string?> SaveAsync(WorkspaceModel workspace, string? path = null)
        {
            path ??= Path.Combine(PathHelper.DocumentsPath, workspace.Name + ".tcw");

            // Get or create entry
            if (!_record.Workspaces.TryGetValue(workspace.Id, out WorkspaceEntryModel? entryModel))
            {
                entryModel = new WorkspaceEntryModel
                {
                    Name = workspace.Name,
                    Path = path,
                };

                _record.Workspaces.Add(workspace.Id, entryModel);
                _record.LastWorkspace = workspace.Id;
            }

            // Save
            await _fileHandler.SaveWorkspace(workspace, path!);
            await _fileHandler.SaveWorkspaceRecord(_record);

            Log.Information($"Saved workspace '{workspace.Name}' to:\n{entryModel.Path}", LoggingCategory);

            return path;
        }

        public async Task<string?> RenameAsync(WorkspaceModel workspace)
        {
            // Get or create entry
            if (!_record.Workspaces.TryGetValue(workspace.Id, out WorkspaceEntryModel? entryModel))
            {
                await SaveAsync(workspace);
                return null;
            }

            string newPath = Path.Combine(PathHelper.DocumentsPath, workspace.Name + ".tcw");

            // Rename
            await _fileHandler.RenameFileAsync(entryModel.Path, workspace.Name);

            // Update
            _record.Workspaces[workspace.Id].Name = workspace.Name;
            _record.Workspaces[workspace.Id].Path = newPath;

            // Save
            await _fileHandler.SaveWorkspace(workspace, newPath);
            await _fileHandler.SaveWorkspaceRecord(_record);

            Log.Information($"Renamed workspace '{entryModel.Name}' to '{workspace.Name}'", LoggingCategory);

            return newPath;
        }

        public void Dispose()
        {
            _record = null!;
        }

        Task IRepository<WorkspaceModel>.SaveAsync(WorkspaceModel item)
        {
            return SaveAsync(item);
        }
    }
}
