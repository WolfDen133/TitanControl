using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Views.Page
{
    public class BasePage : UserControl
    {
        public PageId Id { get; set; }

        public virtual void OnShow() { }
        public virtual void OnHide() { }
        public virtual void OnRegister() { }
    }
}
