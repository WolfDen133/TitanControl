using Avalonia.Controls;
using System.Threading.Tasks;
using TitanControl.Views;

namespace TitanControl.Services.Dialog
{
    public class DialogService : IDialogService
    {
        private Window _parent;

        public DialogService(Window parent)
        {
            _parent = parent;
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var dialog = new ModalDialogWindow
            {
                Title = $"{AppConstants.AppName} - {title}",
                Heading = title,
                Text = message
            };

            return await dialog.ShowDialog<bool>(_parent);
        }
    }
}
