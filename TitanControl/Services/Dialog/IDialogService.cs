using System.Threading.Tasks;

namespace TitanControl.Services.Dialog
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(
            string title, 
            string message, 
            string acceptText, 
            string declineText);
        Task<string?> ShowTextAsync(string title, string message);

        Task ShowMessageAsync(string title, string message, string acceptText);
        Task ShowMessageAsync(string title, string message);

        Task<string?> ShowSaveFileAsync(string title, string sugguested);
        Task<string?> ShowOpenFileAsync(string title);
    }
}
