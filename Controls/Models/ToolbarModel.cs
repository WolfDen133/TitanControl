using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.ViewModels;

namespace TitanControl.Controls.Models
{
    class ToolbarModel : BaseViewModel
    {
        public string Title => AppConstants.AppName;
        public string Version => AppConstants.AppVersion.ToString();
        public string Author => AppConstants.Author;

        public string ActionLabel { get; set; } = "Welcome Avo user";
        public string ActionDescription { get; set; } = "Happy programming!";
    }
}
