using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModel;

namespace TitanControl.ViewModels.Controls
{
    public class InfoModel : BaseViewModel
    {
        private IWorkspaceService _workspaceService;
        private ISessionService _sessionService;

        public InfoModel(IWorkspaceService workspaceService, ISessionService sessionService) 
        {
            _workspaceService = workspaceService;
            _sessionService = sessionService;

            workspaceService.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(workspaceService.CurrentWorkspace))
                    return;

                OnPropertyChanged(nameof(Workspace));

                workspaceService.CurrentWorkspace.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName != nameof(workspaceService.CurrentWorkspace.Name))
                        return;

                    OnPropertyChanged(nameof(Workspace));
                };
            };

            sessionService.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(sessionService.CurrentSession))
                    return;

                OnPropertyChanged(nameof(Session));
                OnPropertyChanged(nameof(SessionState));

                sessionService.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName != nameof(sessionService.CurrentSession))
                        return;

                    switch (args.PropertyName)
                    {
                        case nameof(sessionService.CurrentSession.Name):
                            OnPropertyChanged(nameof(Session));
                            break;
                        case nameof(sessionService.CurrentSession.State):
                            OnPropertyChanged(nameof(SessionState));
                            break;
                    }
                };
            };
        }

        public string Version => AppConstants.AppVersion.ToString();
        public string Author => AppConstants.Author;

        public string? Workspace => _workspaceService.CurrentWorkspace.Name;
        public string? Session => _sessionService.CurrentSession?.Name;

        public SessionConnectionState SessionState => _sessionService.CurrentSession?.State ?? SessionConnectionState.Disabled;

        public string TitleBegining => AppConstants.AppName.Substring(0, 5);
        public string TitleEnding => AppConstants.AppName.Substring(6, 7);
    }
}
