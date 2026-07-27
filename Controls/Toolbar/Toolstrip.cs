using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Toolbar.Buttons;

namespace TitanControl.Controls.Toolbar
{
    public class Toolstrip : StackPanel
    {
        public Dictionary<int, ToolbarButton> MenuTree = new Dictionary<int, ToolbarButton>();

        public static int MaxPerPage = 6;

        public int Current = -1;

        public bool Exclusive = false;

        public Toolstrip() : base()
        {
            LoadMenuItems();

            LoadDefaultPage();

            FlowDirection = Avalonia.Media.FlowDirection.LeftToRight;
            Orientation = Avalonia.Layout.Orientation.Horizontal;
        }

        protected virtual void LoadMenuItems()
        {
            MenuTree.Add(-1, new BackButton() { Toolstrip = this });

            foreach (ToolbarButton item in MenuTree.Values)
            {
                item.PointerReleased += (object? sender, PointerReleasedEventArgs e) =>
                {
                    if (Exclusive)
                    {
                        foreach (var button in MenuTree.Values)
                        {
                            if (button != item) button.ReleaseToggle();
                        }
                    }
                };
            }
        }

        /* TODO
        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            WorkspaceView.Instance?.EnableEditMode(false);
        }*/

        protected virtual void LoadDefaultPage()
        {
            Current = -1;
        }

        public void LoadPage(int index)
        {
            if (index == 0)
            {
                LoadDefaultPage();
                return;
            }

            if (MenuTree[index].ID == -1)
            {
                LoadPage(0);
                return;
            }

            if (MenuTree[index].Children == null) return;

            Children.Clear();

            Children.Add(MenuTree[-1]);

            foreach (int child in MenuTree[index].Children)
            {
                Children.Add(MenuTree[child]);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
                return finalSize;

            double buttonSize = finalSize.Height;
            double x = 0;

            foreach (var child in Children)
            {
                child.Arrange(new Rect(
                    x,
                    0,
                    buttonSize,
                    buttonSize));

                x += buttonSize;
            }

            return finalSize;
        }
    }

}

