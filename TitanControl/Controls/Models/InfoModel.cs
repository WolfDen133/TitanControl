using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.ViewModels;

namespace TitanControl.Controls.Models
{
    public class InfoModel : BaseViewModel
    {
        public string Version => AppConstants.AppVersion.ToString();
        public string Author => AppConstants.Author;
        public string Status { get; set; } = "Ready";
        public string Workspace { get; set; } = "Default";
        public string Session { get; set; } = "Titan Session";
        public SessionConnectionState SessionStatus
        {
            get; 
            set;
        } = SessionConnectionState.Unreachable;

        public string TitleBegining => AppConstants.AppName.Substring(0, 5);
        public string TitleEnding => AppConstants.AppName.Substring(6, 7);

        public void UpdateSessionState(SessionConnectionState state)
        {
            SessionStatus = state;
            OnPropertyChanged(nameof(SessionStatus));
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }
    }
}
