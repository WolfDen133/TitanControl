using Avalonia.Controls.Converters;
using TitanControl.Models.Control;
using TitanControl.Services.Session;

namespace TitanControl.ViewModels.Workspace.Handle
{
    public class HandleButtonModel : HandleControlModel<ButtonControlModel>
    {
        public string Color { get; set; } = string.Empty;
        public string Legend { get; set; } = string.Empty;

        public HandleButtonModel(ButtonControlModel model, ISessionService service) : base(model, service)
        {

        }
    }
}
