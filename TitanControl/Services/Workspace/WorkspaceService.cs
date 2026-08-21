using Avalonia.Markup.Xaml.MarkupExtensions;
using ExCSS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Disk;
using TitanControl.Disk.Resporitory.Workspace;
using TitanControl.Events.Workspace;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Models.Workspace;

namespace TitanControl.Services.Workspace
{
    public class WorkspaceService : IWorkspaceService
    {
        private const string LoggingCategory = "Workspace Service";

        private bool _disposed = false;

        private WorkspaceModel? _currentWorkspace = null;
        private IWorkspaceRepository _workspaceRepo;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<WorkspaceEventArgs>? WorkspaceCreated;
        public event EventHandler<WorkspaceEventArgs>? WorkspaceDeleted;
        public event EventHandler<WorkspaceEventArgs>? WorkspaceModified;
        public event EventHandler<WorkspaceEventArgs>? WorkspaceSaved;
        public event EventHandler<WorkspaceEventArgs>? WorkspacedLoaded;

        public WorkspaceService(WorkspaceRepository repo)
        {
            _workspaceRepo = repo;
        }

        public WorkspaceModel CurrentWorkspace
        {
            get => _currentWorkspace
                ?? throw new InvalidOperationException(
                    "No workspace is currently selected.");

            private set
            {
                if (ReferenceEquals(_currentWorkspace, value))
                    return;

                _currentWorkspace = value;
                OnPropertyChanged(nameof(CurrentWorkspace));
                OnPropertyChanged(nameof(HasCurrentWorkspace));
            }
        }

        public bool HasCurrentWorkspace => _currentWorkspace != null;

        public async Task LoadAsync()
        {
            if (!_workspaceRepo.HasLastWorkspace)
            {
                var ex = new InvalidOperationException("Last workspace not specified");
                Log.Error("Cannot load default workspace as no workspace was last used.");
                throw ex;
            }

            await LoadAsync(_workspaceRepo.LastWorkspace);
        }

        public async Task LoadAsync(Guid workspaceId)
        {
            var workspace = await _workspaceRepo.LoadAsync(workspaceId);

            CurrentWorkspace = workspace;

            Log.Information($"Successfully opened {workspace.Name} workspace.", LoggingCategory);
        }

        public async Task SaveAsync()
        {
            await SaveAsync(CurrentWorkspace);
        }

        public async Task SaveAsync(WorkspaceModel workspace)
        {
            await _workspaceRepo.SaveAsync(workspace);

            Log.Information($"Sucessfully saved {workspace.Name} workspace.", LoggingCategory);
        }

        public async Task<WorkspaceModel> Create(string name)
        {
            var workspace = new WorkspaceModel
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

            WorkspaceCreated?.Invoke(this, new WorkspaceEventArgs(workspace));

            // Save current workspace
            var previous = CurrentWorkspace;
            await SaveAsync(previous);

            // Open new workspace
            CurrentWorkspace = workspace;

            // Save new workspace
            await SaveAsync();

            Log.Information($"Sucessfully created new workspace: {workspace.Id} ({workspace.Id})");

            return workspace;
        }

        public Task Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<WorkspaceModel> Get(Guid id)
        {
            return await _workspaceRepo.LoadAsync(id);
        }

        public void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        public void Dispose()
        {
            _workspaceRepo.Dispose();

            CurrentWorkspace = null!;
            _disposed = true;
        }
    }
}