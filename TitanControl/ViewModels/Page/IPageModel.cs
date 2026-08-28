using Avalonia.Controls;
using System.Threading.Tasks;

namespace TitanControl.ViewModels.Page
{
    public interface IPageModel : IViewModel
    {
        PageId Id { get; }

        Task OnOpenAsync()
        {
            return null!;
        }

        Task OnCloseAsync()
        {
            return null!;
        }
    }
}
