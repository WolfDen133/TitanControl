using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Toolbar.Buttons.ContextToolstrip;

namespace TitanControl.Controls.Toolbar
{
    public class ContextToolstrip : Toolstrip
    {
        public ContextToolstrip()
        {
            Exclusive = true;
        }

        protected override void LoadMenuItems()
        {
            MenuTree.Add(1, new AddButton() { Toolstrip = this});
            MenuTree.Add(2, new CopyButton() {Toolstrip = this});
            MenuTree.Add(3, new MoveButton() {Toolstrip = this});
            MenuTree.Add(4, new AssignButton() {Toolstrip = this});
            MenuTree.Add(5, new PropertiesButton() {Toolstrip = this});
            MenuTree.Add(6, new RemoveButton() {Toolstrip = this});
            MenuTree.Add(11, new AddButtonButton() {Toolstrip = this});
            MenuTree.Add(12, new AddFaderButton() {Toolstrip = this});
            MenuTree.Add(13, new AddColorPickerButton() {Toolstrip = this});

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
            Children.Add(MenuTree[1]);
            Children.Add(MenuTree[2]);
            Children.Add(MenuTree[3]);
            Children.Add(MenuTree[4]);
            Children.Add(MenuTree[5]);
            Children.Add(MenuTree[6]);
        }
    }
}
