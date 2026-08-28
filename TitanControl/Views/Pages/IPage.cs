using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.ViewModels.Page;

namespace TitanControl.Views.Pages
{
    public interface IPage
    {
        PageId Id { get; }
        PageState State { get; set; }
        Dock Dock { get; }
        bool IsActive { get; set; }
    }
}
