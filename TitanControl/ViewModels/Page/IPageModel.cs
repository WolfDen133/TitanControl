using Avalonia.Controls;
using System;
using System.Threading.Tasks;

namespace TitanControl.ViewModels.Page
{
    public interface IPageModel : IViewModel
    {
        PageId Id { get; }

        event PageRequestHandler? RequestOpen;
        event PageRequestHandler? RequestClose;

        Task OnOpenAsync();

        Task OnCloseAsync();
    }

    public delegate Task PageRequestHandler(IPageModel page);
}
