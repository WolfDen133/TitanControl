using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Events;
using TitanControl.Logging;
using TitanControl.Models.Workspace;
using TitanControl.Services.Session;
using TitanControl.ViewModel;
using TitanControl.ViewModels.Workspace;
using TitanControl.WebAPI;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Data.Model;

namespace TitanControl.ViewModels.Page.HandleBrowser
{
    public partial class HandleBrowserModel : BasePageModel, IDialog
    {
        private const string LoggingCategory = "HandleBrowser ViewModel";

        private ISessionService _sessionService;
        private List<Handle> _handles = new();
        private ObservableCollection<Handle> _displayHandles = new();
        private HandleType _currentTab = HandleType.None;
        private BrowserMode _mode = BrowserMode.Assign;
        private ISession? CurrentSession => _sessionService.CurrentSession;

        private WorkspaceViewModel _workspace;

        public event EventHandler<DialogClosedEventArgs>? DialogClosed;

        public ObservableCollection<Handle> Handles
        {
            get => _displayHandles;
            set => SetProperty(ref _displayHandles, value);
        }

        public HandleType CurrentTab
        {
            get => _currentTab;
            set => SetProperty(ref _currentTab, value);
        }

        public BrowserMode Mode
        {
            get => _mode;
            set
            {
                SetProperty(ref _mode, value);
                OnPropertyChanged(nameof(ButtonText));
                OnPropertyChanged(nameof(ButtonIcon));
            }
        }
        public override PageId Id => PageId.HandleBrowser;

        public ObservableCollection<HandleCount> HandleCount { get; } = new();

        public int Count => GetHandleCount(_currentTab);
        public int TotalCount => GetHandleCount();
        public string CountDisplay => 
            (_currentTab != HandleType.None 
             ? $"Showing {Count} / {TotalCount}" 
             : $"Loaded {TotalCount}") 
            + " handles";

        public string ButtonText => Mode switch
        {
            BrowserMode.Assign => "Assign",
            BrowserMode.Import => "Import",
            _ => throw new NotImplementedException()
        };

        public string ButtonIcon => 
            "/Assets/Icons/" 
            + Mode switch
              {
                  BrowserMode.Assign => "trigger",
                  BrowserMode.Import => "shift-up",
                  _ => throw new NotImplementedException()
              } 
            + ".svg"; 

        public HandleBrowserModel(ISessionService sessionService, WorkspaceViewModel workspace)
        {
            _sessionService = sessionService;
            _workspace = workspace;

            Handles.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(CountDisplay));
            };
        }


        public void OnCancel()
        {
            _workspace.HandleActionCancel(WorkspaceAction.Assign);
        }

        public void OnAccept()
        {
            _workspace.HandleActionCompleted();
        }

        public override Task InitializeAsync()
        {
            Mode = BrowserMode.Assign;

            return Task.CompletedTask;
        }

        public override async Task OnOpenAsync()
        {
            await base.OnOpenAsync();

            if (!_sessionService.CurrentSession?.IsConnected ?? true)
                return;

            await RegisterHandles();

            Log.Debug($"Loaded {_handles.Count} handels, {Handles.Count} types.", LoggingCategory);

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(CountDisplay));

            if (Handles.Count > 0)
                await ShowHandles(Handles.FirstOrDefault()!.Type);
        }
        
        private async Task RegisterHandles()
        {
            _handles.Clear();
            HandleCount.Clear();

            var handles = await CurrentSession!.Api!.Handles.GetHandles();

            if (handles == null)
            {
                var ex = new InvalidDataException("Handles returned null");
                Log.Error(ex, $"Could get models as the no handles were returned", LoggingCategory);
                throw ex;
            }

            foreach (var handle in handles)
            {
                if (handle is null)
                    continue;

                _handles.Add(handle);
            }

            foreach (var group in _handles.GroupBy(x => x.Type))
            {
                HandleCount.Add(new HandleCount
                {
                    Type = group.Key,
                    Count = group.Count()
                });
            }

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(CountDisplay));
        }

        public override Task OnCloseAsync()
        {
            _handles.Clear();
            Handles.Clear();

            return base.OnCloseAsync();
        }

        public Task ShowTypeHandles(HandleType type)
        {
            Handles = [.. _handles.Where(h => h.Type == type)];
            CurrentTab = type;

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(CountDisplay));

            return Task.CompletedTask;
        }
        public int GetHandleCount(HandleType? type = null)
        {
            if (type == null || type == HandleType.None)
                return HandleCount.Sum(h => h.Count);

            return HandleCount.FirstOrDefault(h => h.Type == type)?.Count ?? 0;
        }

        [RelayCommand]
        public async Task ShowHandles(HandleType type) 
            => await ShowTypeHandles(type);

        [RelayCommand]
        public void Select()
        {
            ClosePage(false);
            DialogClosed?.Invoke(this, new DialogClosedEventArgs<List<Handle>>
            {
                Result = [.. Handles.Where(h => h.Selected)]
            });
        }

        [RelayCommand]
        public void Close()
        {
            ClosePage(true);
            DialogClosed?.Invoke(this, new DialogClosedEventArgs<List<Handle>>()
            {
                IsCanceled = true,
            });
        }

        public void ClosePage(bool cancelled = false)
        {
            if (cancelled)
                OnCancel();
            else
                OnAccept();
        }


        public override ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class HandleCount
    {
        public HandleType Type { get; init; }
        public int Count { get; set; }
    }
}
