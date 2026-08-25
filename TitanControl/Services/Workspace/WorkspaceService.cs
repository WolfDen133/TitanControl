using System;
using System.ComponentModel;
using System.Threading.Tasks;
using TitanControl.Disk.Resporitory.Workspace;
using TitanControl.Events.Workspace;
using TitanControl.Logging;
using TitanControl.Models.Workspace;

namespace TitanControl.Services.Workspace
{
    public class WorkspaceService : IWorkspaceService, IAsyncDisposable
    {
        private const string LoggingCategory = "Workspace Service";

        private bool _disposed = false;

        private WorkspaceModel? _currentWorkspace = null;
        private IWorkspaceRepository _workspaceRepo;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<WorkspaceEventArgs>? WorkspaceCreated;
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
                OnPropertyChanged(nameof(HasWorkspace));
            }
        }

        public bool HasWorkspace => _currentWorkspace != null;
        public bool HasLastWorkspace => _workspaceRepo.LastWorkspace != Guid.Empty;

        public string[] WorkspaceNames => _workspaceRepo.WorkspaceNames;

        public async Task InitializeAsync()
        {
            ThrowIfDisposed();

            await _workspaceRepo.LoadRecord();
        }

        public async Task LoadAsync()
        {
            ThrowIfDisposed();

            if (!HasLastWorkspace)
            {
                var ex = new InvalidOperationException("Last workspace not specified");
                Log.Error("Cannot load default workspace as no workspace was last used.");
                throw ex;
            }

            await LoadAsync(_workspaceRepo.LastWorkspace);
        }

        public async Task LoadAsync(Guid workspaceId)
        {
            ThrowIfDisposed();

            var workspace = await _workspaceRepo.LoadAsync(workspaceId);

            CurrentWorkspace = workspace;

            WorkspacedLoaded?.Invoke(this, new WorkspaceEventArgs(workspace));

            Log.Information($"Successfully opened {workspace.Name} workspace.", LoggingCategory);
        }

        public async Task SaveAsync()
        {
            ThrowIfDisposed();

            await SaveAsync(CurrentWorkspace);
        }

        public async Task SaveAsync(WorkspaceModel workspace)
        {
            ThrowIfDisposed();

            await _workspaceRepo.SaveAsync(workspace);

            WorkspaceSaved?.Invoke(this, new WorkspaceEventArgs(workspace));

            Log.Information($"Sucessfully saved {workspace.Name} workspace.", LoggingCategory);
        }

        public async Task<WorkspaceModel> Create(string name)
        {
            ThrowIfDisposed();

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
            if (HasWorkspace)
            {
                var previous = CurrentWorkspace;
                await SaveAsync(previous);
            }

            // Open new workspace
            CurrentWorkspace = workspace;

            // Save new workspace
            await SaveAsync();

            Log.Information($"Sucessfully created new workspace: {workspace.Id} ({workspace.Id})");

            return workspace;
        }

        public Task Delete(Guid id)
        {
            ThrowIfDisposed();

            throw new NotImplementedException();
        }

        public async Task<WorkspaceModel> Get(Guid id)
        {
            ThrowIfDisposed();

            if (CurrentWorkspace.Id == id)
                return CurrentWorkspace;

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

        public ValueTask DisposeAsync()
        {
            _workspaceRepo.Dispose();

            CurrentWorkspace = null!;
            _disposed = true;

            return ValueTask.CompletedTask;
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
    }
}