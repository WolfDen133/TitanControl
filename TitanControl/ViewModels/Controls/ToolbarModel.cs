using System;
using System.Threading.Tasks;
using TitanControl.Events.Control;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModel;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.Views.Controls.Toolbar.Buttons;

namespace TitanControl.ViewModels.Controls
{
    public class ToolbarModel : BaseViewModel
    {
        public event EventHandler<ToolButtonPressedEventArgs>? ButtonClicked;

        public InfoModel InfoModel { get; set; }

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

        public void OnButtonClicked(ButtonId button, ButtonAction action)
        {
            ButtonClicked?.Invoke(this, new ToolButtonPressedEventArgs
            {
                ButtonId = button,
                ButtonAction = action
            });
        }
    } 
}
