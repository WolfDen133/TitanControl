using TitanControl.Services.Session;
using TitanControl.ViewModel;

namespace TitanControl.ViewModels.Controls
{
    public class InfoModel : BaseViewModel
    {
        private string _session = "Titan Session";
        private string _workspace = "Default";
        private string _status = "Ready";
        private SessionConnectionState _state = SessionConnectionState.Unreachable;

        public string Version => AppConstants.AppVersion.ToString();
        public string Author => AppConstants.Author;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value, nameof(Status));

        }
        public string Workspace
        {
            get => _workspace;
            set => SetProperty(ref _workspace, value, nameof(Workspace));
        }
        public string Session
        {
            get => _session;
            set => SetProperty(ref _session, value, nameof(Session));
        }

        public SessionConnectionState SessionState
        {
            get => _state;
            set => SetProperty(ref _state, value, nameof(SessionState));
        }

        public string TitleBegining => AppConstants.AppName.Substring(0, 5);
        public string TitleEnding => AppConstants.AppName.Substring(6, 7);
    }
}
