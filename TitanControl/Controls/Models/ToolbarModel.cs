using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Toolbar.Buttons;
using TitanControl.Controls.Toolbar.Event;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModels;

namespace TitanControl.Controls.Models
{
    public class ToolbarModel : BaseViewModel
    {
        private SessionService _sessionService;
        private WorkspaceService _workspaceService;

        public event EventHandler<ToolButtonPressedEventArgs>? ButtonClicked;

        public InfoModel InfoModel { get; set; }

        public ToolbarModel(SessionService sessionService, WorkspaceService workspaceService)
        {
            _sessionService = sessionService;
            _workspaceService = workspaceService;

            InfoModel = new InfoModel();

            sessionService.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(sessionService.CurrentSession))
                {
                    sessionService.CurrentSession?.StateChanged += (_, stateEventArgs) =>
                    {
                        InfoModel.UpdateSessionState(stateEventArgs.CurrentState);
                    };
                }
            };
        }

        public void OnButtonClicked(ButtonId button, ToolbarButton.ButtonAction action)
        {
            ButtonClicked?.Invoke(this, new ToolButtonPressedEventArgs {
                ButtonId = button,
                ButtonAction = action
            });
        }
    }
}
