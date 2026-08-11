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

namespace TitanControl.Workspaces
{
    public class WorkspaceManager : INotifyPropertyChanged
    {
        private Workspace currentWorkspace = null!;

        private WorkspaceRecordModel record = new WorkspaceRecordModel();

        public event PropertyChangedEventHandler? PropertyChanged;

        public Workspace CurrentWorkspace { 
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

        public async Task<Workspace> LoadLastWorkspace()
        {
            return await Load(record.LastWorkspace);
        }

        public async Task<Workspace> Load(Guid id)
        {
            if (!record.Workspaces.TryGetValue(id, out var workspaceEntry))
            {
                var ex = new InvalidDataException("Workspace could not be found");
                Log.Error(ex, $"Workspace with key {id} could not be found in record.");
                throw ex;
            }

            string path;
            if (record.Workspaces[id].Path != string.Empty)
                path = record.Workspaces[id].Path;
            else
                path = Path.Combine(PathHelper.DocumentsPath, workspaceEntry.Name + ".tcw");

            if (!File.Exists(path))
            {
                var ex = new FileNotFoundException("Workspace file could not be found", path);
                Log.Error(ex, $"Workspace file could not be found at {path}");
                throw ex;
            }

            var workspaceModel = await FileHandler.LoadWorkspace(path);

            CurrentWorkspace = (Workspace)workspaceModel.ToInstance();
            record.LastWorkspace = id;

            await SaveRecord();

            return CurrentWorkspace;
        } 

        public async Task Save(Workspace workspace)
        {
            if (!record.Workspaces.TryGetValue(workspace.Id, out var workspaceEntry))
            {
                workspaceEntry = new WorkspaceEntryModel
                {
                    Name = workspace.Name,
                    Path = Path.Combine(PathHelper.DocumentsPath, workspace.Name + ".tcw")
                };

                record.Workspaces.Add(workspace.Id, workspaceEntry);
            }

            await FileHandler.SaveWorkspace((WorkspaceModel)workspace.ToModel(), workspaceEntry.Path);

            await SaveRecord();
        }

        private async Task LoadRecord()
        {
            record = await FileHandler.LoadWorkspaceRecord();
        }

        public async Task SaveRecord()
        {
            await FileHandler.SaveWorkspaceRecord(record);
        }

        public Workspace Create(string name)
        {
            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = name,
                Options = new WorkspaceOptions
                {
                    Session = Guid.Empty,
                    GridSize = new System.Drawing.Size(12, 12)
                },
                Controls = new(),
                LastModified = DateTime.Now,
            };

            CurrentWorkspace = workspace;
            record.LastWorkspace = workspace.Id;

            _ = Save(workspace);
            
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
