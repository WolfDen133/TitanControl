using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.ViewModel;

namespace TitanControl.ViewModels.Page
{
    public abstract class BasePageModel : BaseViewModel, INotifyPropertyChanged, IAsyncDisposable, IPageModel
    {
        public virtual PageId Id { get; } = PageId.None;

        public abstract Task OnOpenAsync();
        public abstract Task OnCloseAsync();
        public abstract ValueTask DisposeAsync();
    }
}
