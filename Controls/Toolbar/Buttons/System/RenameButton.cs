
using sToolstrip = TitanControl.Controls.Toolbar.SystemToolstrip;

namespace TitanControl.Controls.Toolbar.Buttons.SystemToolstrip
{
    class RenameButton : ToolbarButton
    {
        public RenameButton() : base()
        {
            ID = 34;
            Text = "Rename";
            Description = "Rename your current worspace.";

            ButtonImage.Source = ToImage("rename.png");
        }
    }
}
