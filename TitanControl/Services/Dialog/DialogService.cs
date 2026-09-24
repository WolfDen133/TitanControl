using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Platform.Storage;
using ExCSS;
using System.Linq;
using System.Threading.Tasks;
using TitanControl.Helper;
using TitanControl.Views;

namespace TitanControl.Services.Dialog
{
    public class DialogService : IDialogService
    {
        private readonly Window _parent;

        public DialogService(Window parent)
        {
            _parent = parent;
        }

        public async Task<bool> ShowConfirmationAsync(
            string title, 
            string message, 
            string acceptText = "Accept", 
            string declineText = "Decline")
        {
            var dialog = new ModalDialogWindow() { ParentWindow = _parent };

            return await dialog.ShowDialog<bool>(
                title, 
                message, 
                acceptText, 
                declineText, 
                true);
        }

        public async Task ShowMessageAsync(
            string title,
            string message,
            string acceptText = "Accept")
        {
            var dialog = new ModalDialogWindow() { ParentWindow = _parent };

            await dialog.ShowMessage(
                title,
                message,
                acceptText);
        }

        public async Task ShowMessageAsync(
            string title,
            string message)
        {
            var dialog = new ModalDialogWindow() { ParentWindow = _parent };

            await dialog.ShowMessage(
                title,
                message,
                "Accept");
        }

        public async Task<string?> ShowTextAsync(string title, string message)
        {
            var dialog = new TextDialogWindow() { ParentWindow = _parent };

            return await dialog.ShowDialog(title, message);
        }

        public async Task<string?> ShowSaveFileAsync(string title, string suggested)
        {
            var toplevel = TopLevel.GetTopLevel(_parent);

            var file = await toplevel!.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = title,
                    FileTypeChoices = 
                    [
                        new("TitanControl Workspace") { Patterns = [ "*.tcw", ".json" ] }
                    ],
                    ShowOverwritePrompt = true,
                    SuggestedFileName = suggested,
                    SuggestedStartLocation = await toplevel.StorageProvider.TryGetFolderFromPathAsync(PathHelper.DocumentsPath)
                }
            );

            if (file is null)
                return null;

            return file.Path.LocalPath;
        }

        public async Task<string?> ShowOpenFileAsync(string title)
        {
            var toplevel = TopLevel.GetTopLevel(_parent);

            var file = await toplevel!.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = title,
                    FileTypeFilter =
                    [
                        new("TitanControl Workspace") { Patterns = [ "*.tcw", ".json"] }
                    ],
                    AllowMultiple = false,
                }
            );

            if (file is null || file.Count < 1)
                return null;

            Logging.Log.Debug(file[0].Path.ToString());

            return file[0]!.Path.LocalPath;
        }
    }
}
