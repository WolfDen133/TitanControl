using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.ViewModels;
using TitanControl.ViewModels.Page;
using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class SessionViewButton : ToolbarButton
    {
        public SessionViewButton() : base()
        {
            ID = 21;
            Text = "Session View";
            Description = "Opens the session configuration window (this is how you connect to titan).";
            Toggle = true;

            ButtonSvg.Path = "/Assets/Icons/session.svg";
        }

        protected override void ClickAction(ButtonAction action)
        {
            if (action == ButtonAction.ToggleDown)
            {
                MainWindow.PageManager.ShowPage(PageId.Session);
                return;
            }

            MainWindow.PageManager.ShowPage(PageId.Workspace);
        }
    }
}
