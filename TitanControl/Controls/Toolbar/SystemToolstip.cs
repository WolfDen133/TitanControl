using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Toolbar.Buttons.SystemToolstrip;

namespace TitanControl.Controls.Toolbar
{
    public class SystemToolstrip : Toolstrip
    {
        public SystemToolstrip()
        {
            Exclusive = true;

            FlowDirection = Avalonia.Media.FlowDirection.RightToLeft;
        }

        protected override void LoadMenuItems()
        {
            base.LoadMenuItems();

            MenuTree.Add(1, new SettingsButton() { Toolstrip = this });
            MenuTree.Add(2, new DiskButton() { Toolstrip = this });
            MenuTree.Add(3, new WindowsButton() { Toolstrip = this });

            MenuTree.Add(11, new FullScreenButton() { Toolstrip = this });
            MenuTree.Add(12, new SessionViewButton() { Toolstrip = this });

            MenuTree.Add(21, new SaveButton() { Toolstrip = this });
            MenuTree.Add(22, new SaveAsButton() { Toolstrip = this });
            MenuTree.Add(23, new LoadButton() { Toolstrip = this });
            MenuTree.Add(24, new RenameButton() { Toolstrip = this });
            MenuTree.Add(25, new NewButton() { Toolstrip = this });

            MenuTree.Add(31, new CircleBuilderButton() { Toolstrip = this });
            // MenuTree.Add(43, new SettingsButton(this));

            MenuTree[-1].FlowDirection = Avalonia.Media.FlowDirection.RightToLeft; 
        }

        public void SoftRelease()
        {
            foreach (var item in MenuTree.Values)
            {
                item.ReleaseToggle(true);
            }
        }

        protected override void ShowDefaultButtons()
        {
            SetButtonVisible(1);
            SetButtonVisible(2);
            SetButtonVisible(3);
        }
    }
}
