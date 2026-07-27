using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            ButtonImage.Source = ToImage("laptop.png");
        }

        protected override void ClickAction(ButtonAction action)
        {
            App.SessionManager.TryGet("Default Session", out var session);

            session?.Start();
        }
    }
}
