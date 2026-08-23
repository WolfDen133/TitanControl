using System.Threading.Tasks;

namespace TitanControl.Services.Dialog
{
    public interface IDialogService
    {

        Task<bool> ShowConfirmationAsync(
            string title, string message);

    }
}
