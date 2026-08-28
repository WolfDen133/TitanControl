using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.ViewModels.Page;

namespace TitanControl.Views.Pages
{
    public class BasePage : UserControl, IPage
    {
        public static readonly StyledProperty<PageState> StateProperty =
            AvaloniaProperty.Register<BasePage, PageState>(nameof(State), PageState.Closed);

        public virtual PageId Id => PageId.None;

        public PageState State 
        { 
            get => GetValue(StateProperty); 
            set => SetValue(StateProperty, value); 
        }

        public virtual Dock Dock => Dock.Top;

        public bool IsActive { get; set; }
    }
}
