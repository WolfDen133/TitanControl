using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Events.Control;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModel;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;

namespace TitanControl.ViewModels.Controls.Toolbar
{
    public class ToolbarModel : BaseViewModel
    {
        public event EventHandler<ToolButtonPressedEventArgs>? ButtonClicked;

        public InfoModel InfoModel { get; set; }

        public List<ButtonId> AvailableContext = 
        [
            ButtonId.Move,
            ButtonId.Copy,
            ButtonId.Assign,
            ButtonId.Options,
            ButtonId.Remove
        ];

        public ObservableCollection<ToolbarButtonModel> ContextButtons { get; } =
        [
            new(ButtonId.Back),
            new(ButtonId.Add) {
                Children = [
                    ButtonId.AddButton,
                    ButtonId.AddFader,
                    ButtonId.AddColorPicker
                ]
            },
            new(ButtonId.AddButton)
            {
                IsToggle = true
            },
            new(ButtonId.AddFader)
            {
                IsToggle = true,
            },
            new(ButtonId.AddColorPicker) 
            {
                IsToggle = true
            },
            new(ButtonId.Copy) 
            {
                IsToggle = true
            },
            new(ButtonId.Move)
            {
                IsToggle = true,
            },
            new(ButtonId.Assign) 
            {
                IsToggle = true,
            },
            new(ButtonId.Options) 
            {
                IsToggle = true
            },
            new(ButtonId.Remove) 
            {
                IsToggle = true
            }
        ];

        public ObservableCollection<ToolbarButtonModel> SystemButtons { get; } =
        [
            new(ButtonId.Back, Avalonia.Media.FlowDirection.RightToLeft),
            new(ButtonId.Config)
            {
                Children = [
                    ButtonId.Fullscreen,
                    ButtonId.Sessions
                ]
            },
            new(ButtonId.Fullscreen){
                IsToggle = true,
            },
            new(ButtonId.Sessions){
                IsToggle = true,
            },
            new(ButtonId.Windows)
            {
                Children = [ButtonId.CircleBuilder]
            },
            
            new(ButtonId.CircleBuilder){
                IsToggle = true,
            },
            new(ButtonId.Disk)
            {
                Children = [
                    ButtonId.Save,
                    ButtonId.SaveAs,
                    ButtonId.Load,
                    ButtonId.Rename,
                    ButtonId.New
                ]
            },
            new(ButtonId.Latch)
            {
                IsToggle = true
            },
            new(ButtonId.Save) {
                IsToggle = true
            },
            new(ButtonId.SaveAs) {
                IsToggle = true
            },
            new(ButtonId.Load){
                IsToggle = true,
            },
            new(ButtonId.Rename) {
                IsToggle = true,
            },
            new(ButtonId.New) {
                IsToggle = true
            }
        ];

        public ToolbarModel(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            InfoModel = new InfoModel();

            sessionService.PropertyChanged += SessionService_PropertyChanged;

            workspaceService.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(workspaceService.CurrentWorkspace))
                    return;

                InfoModel.Workspace = workspaceService.CurrentWorkspace?.Name!;
            };
        }

        private void SessionService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not ISessionService service || e.PropertyName != nameof(service.CurrentSession))
                return;

            if (service.CurrentSession == null)
                Log.Debug("Current session is null");
            else
            {
                InfoModel.SessionState = service.CurrentSession.State;
                Log.Debug($"Session {service.CurrentSession} is now {service.CurrentSession.State}");
            }

            service.CurrentSession?.StateChanged += (_, stateEventArgs) =>
            {
                InfoModel.SessionState = stateEventArgs.CurrentState;
                Log.Debug($"Session {service.CurrentSession} is now {stateEventArgs.CurrentState}");
            };

            InfoModel.Session = service.CurrentSession?.Name!;
        }

        public void ReleaseToggleSoft(ButtonId buttonId)
        {
            var button = ContextButtons
                .Concat(SystemButtons)
                .FirstOrDefault(button => button.Id == buttonId)
                ?? throw new InvalidOperationException(
                    $"No button found with ID {buttonId}");

            button.Toggled = false;
        }

        public void ShowAvailable(bool show = true)
        {
            foreach (var button in ContextButtons.Where(b => AvailableContext.Contains(b.Id)))
                button.IsAvailable = show;
        }
    } 
}
