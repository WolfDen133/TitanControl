using System;
using TitanControl.Controls.Toolbar.Buttons;
using TitanControl.Controls.Toolbar.Event;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModels;

namespace TitanControl.Controls.Models
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
                if (args.PropertyName == nameof(sessionService.CurrentSession))
                {
                    sessionService.CurrentSession?.StateChanged += (_, stateEventArgs) =>
                    {
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
