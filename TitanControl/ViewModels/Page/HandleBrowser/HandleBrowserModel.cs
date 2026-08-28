using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.ViewModel;
using TitanControl.WebAPI;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Data.Model;

namespace TitanControl.ViewModels.Page.HandleBrowser
{
    public partial class HandleBrowserModel : BasePageModel
    {
        private const string LoggingCategory = "HandleBrowser ViewModel";

        private ISessionService _sessionService;
        private List<Handle> _handles = new();
        private ObservableCollection<Handle> _displayHandles = new();
        private HandleType _currentTab = HandleType.None;

        private ISession? CurrentSession => _sessionService.CurrentSession;

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

        public override PageId Id => PageId.HandleBrowser;

        public HandleBrowserModel(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public override async Task InitializeAsync()
        {
            CurrentTab = HandleType.Cue;
        }

        public override async Task OnOpenAsync()
        {
            if (_sessionService.CurrentSession?.IsConnected ?? true)
                return;

            _handles.Clear();

            var handles = await _sessionService.CurrentSession!.Api!.Handles.GetHandles();

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

            Log.Debug($"Loaded {_handles.Count} handels.", LoggingCategory);
        }

        public override async Task OnCloseAsync()
        {
            _handles.Clear();
            Handles.Clear();
        }

        public async Task ShowTypeHandles(HandleType type)
        {
            Handles = [.. _handles.Where(h => h.Type == type)];
        }

        [RelayCommand]
        public async Task ShowHandles(HandleType type) 
            => await ShowTypeHandles(type);

        public override ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
