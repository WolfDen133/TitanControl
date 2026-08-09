using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk;
using TitanControl.Disk.Model.Workspace;
using TitanControl.Helper;
using TitanControl.Logging;

namespace TitanControl.Workspace
{
    public class WorkspaceManager : INotifyPropertyChanged
    {
        private WorkspaceModel currentWorkspace = null!;

        private WorkspaceRecordModel record = new WorkspaceRecordModel();

        public event PropertyChangedEventHandler? PropertyChanged;

        public WorkspaceModel CurrentWorkspace { 
            get => currentWorkspace; 
            private set
            {
                currentWorkspace = value;
                OnPropertyChanged(nameof(CurrentWorkspace));
            }
        }
      
        public WorkspaceManager() 
        {
            _ = LoadRecord();
        }

        public bool HasLastWorkspace()
        {
            return record.LastWorkspace != Guid.Empty;
        }

        public async Task<WorkspaceModel> Load(Guid id)
        {
            if (!record.Workspaces.TryGetValue(id, out var workspace))
            {
                var ex = new InvalidDataException("Workspace could not be found");
                Log.Error(ex, $"Workspace with key {id} could not be found in record.");
                throw ex;
            }

            string path;
            if (record.Workspaces[id].Path != string.Empty)
                path = record.Workspaces[id].Path;
            else
                path = Path.Combine(PathHelper.DocumentsPath, workspace.Name);

            if (!File.Exists(path))
            {
                var ex = new FileNotFoundException("Workspace file could not be found", path);
                Log.Error(ex, $"Workspace file could not be found at {path}");
                throw ex;
            }

            CurrentWorkspace = await FileHandler.LoadWorkspace(path);
            record.LastWorkspace = id;

            await SaveRecord();

            return CurrentWorkspace;
        } 

        public async Task Save(WorkspaceModel workspace)
        {
            if (!record.Workspaces.TryGetValue(workspace.Id, out var workspaceEntry))
            {
                workspaceEntry = new WorkspaceEntryModel
                {
                    Name = workspace.Name,
                    Path = Path.Combine(PathHelper.DocumentsPath, workspace.Name)
                };

                record.Workspaces.Add(workspace.Id, workspaceEntry);
            }

            await FileHandler.SaveWorkspace(workspace, workspaceEntry.Path);
        }

        private async Task LoadRecord()
        {
            record = await FileHandler.LoadWorkspaceRecord();
        }

        public async Task SaveRecord()
        {
            await FileHandler.SaveWorkspaceRecord(record);
        }

        public WorkspaceModel Create(string name)
        {
            var workspace =  new WorkspaceModel
            {
                Id = Guid.NewGuid(),
                Name = name,
                Options = new WorkspaceOptionsModel
                {
                    Session = Guid.Empty,
                    GridSize = new System.Drawing.Size(12, 12)
                },
                Controls = new(),
                LastModified = DateTime.Now,
            };

            record.Workspaces.Add(workspace.Id, new WorkspaceEntryModel
            {
                Name = workspace.Name,
                Path = string.Empty
            });

            CurrentWorkspace = workspace;

            _ = SaveRecord();

            return workspace;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);

            PropertyChanged?.Invoke(this, e);
        }
    }
}
