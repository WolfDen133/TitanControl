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
        }

        protected override void LoadMenuItems()
        {
            MenuTree.Add(2, new WindowsButton() { Toolstrip = this });
            MenuTree.Add(3, new DiskButton() { Toolstrip = this });
            MenuTree.Add(4, new SettingsButton() { Toolstrip = this });
            MenuTree.Add(21, new CircleBuilderButton() { Toolstrip = this });
            MenuTree.Add(31, new SaveButton() { Toolstrip = this });
            MenuTree.Add(32, new SaveAsButton() { Toolstrip = this });
            MenuTree.Add(33, new LoadButton() { Toolstrip = this });
            MenuTree.Add(34, new RenameButton() { Toolstrip = this });
            MenuTree.Add(35, new NewButton() { Toolstrip = this });
            MenuTree.Add(41, new FullScreenButton() { Toolstrip = this });
            MenuTree.Add(42, new SessionViewButton() { Toolstrip = this });
            // MenuTree.Add(43, new SettingsButton(this));


            base.LoadMenuItems();
        }

        public void SoftRelease()
        {
            foreach (var item in MenuTree.Values)
            {
                item.ReleaseToggle(true);
            }
        }

        protected override void LoadDefaultPage()
        {
            base.LoadDefaultPage();
            Children.Clear();
            Children.Add(MenuTree[4]);
            Children.Add(MenuTree[3]);
            Children.Add(MenuTree[2]);
        }
    }
}
