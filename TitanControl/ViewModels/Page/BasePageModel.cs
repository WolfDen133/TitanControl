using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.ViewModel;
using static TitanControl.ViewModels.Page.IPageModel;

namespace TitanControl.ViewModels.Page
{
    public abstract class BasePageModel : BaseViewModel, INotifyPropertyChanged, IAsyncDisposable, IPageModel
    {
        public virtual PageId Id { get; } = PageId.None;

        public event PageRequestHandler? RequestOpen;
        public event PageRequestHandler? RequestClose;

        public abstract ValueTask DisposeAsync();

        public virtual Task OnOpenAsync()
            => InvokeAsync(RequestOpen);

        public virtual Task OnCloseAsync() 
            => InvokeAsync(RequestClose);

        private async Task InvokeAsync(PageRequestHandler? handlers)
        {
            if (handlers is null)
                return;

            foreach (
                PageRequestHandler handler 
                in handlers
                .GetInvocationList()
                .Cast<PageRequestHandler>())
                await handler(this);
        }
    }
}
