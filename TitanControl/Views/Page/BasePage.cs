using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Views.Page
{
    public class BasePage : UserControl, IPage
    {
        public virtual PageId Id { get; }

        public virtual void OnHide()
        {
            IsVisible = false;
        }

        public virtual void OnRegister()
        {
            IsVisible = false;
        }

        public virtual void OnShow()
        {
            IsVisible = true;
        }
    }
}
