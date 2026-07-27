using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Controls.Models
{
    public class InfoModel
    {
        public string Version => AppConstants.AppVersion.ToString();
        public string Author => AppConstants.Author;
        public string Status { get; set; } = "Ready";
        public string Workspace { get; set; } = "Default";
        public string Session { get; set; } = "Titan Session";
        public string SessionStatus { get; set; } = "Active";


        public string TitleBegining => AppConstants.AppName.Substring(0, 5);
        public string TitleEnding => AppConstants.AppName.Substring(6, 7);
    }
}
