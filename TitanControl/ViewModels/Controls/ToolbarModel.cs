using System;
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

            sessionService.PropertyChanged += (sender, args) =>
            {
                Log.Debug($"A property changed on sessions service {args.PropertyName}");
                if (args.PropertyName == nameof(sessionService.CurrentSession))
                {
                    Log.Debug($"Current session changed {sessionService.CurrentSession?.Name}");
                    sessionService.CurrentSession?.StateChanged += (_, stateEventArgs) =>
                    {
                        Log.Debug($"Current session state changed {stateEventArgs.CurrentState}");
                        InfoModel.SessionState = stateEventArgs.CurrentState;
                    };

                    InfoModel.Session = sessionService.CurrentSession?.Name!;
                }
            };

            workspaceService.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(workspaceService.CurrentWorkspace))
                    InfoModel.Workspace = workspaceService.CurrentWorkspace?.Name!;
            };
        }

        public void OnButtonClicked(ButtonId button, ToolbarButton.ButtonAction action)
        {
            ButtonClicked?.Invoke(this, new ToolButtonPressedEventArgs
            {
                ButtonId = button,
                ButtonAction = action
            });
        }
    }
}
