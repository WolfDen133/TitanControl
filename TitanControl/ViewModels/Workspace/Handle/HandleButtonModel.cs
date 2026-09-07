using Avalonia.Controls.Converters;
using TitanControl.Models.Control;
using TitanControl.Services.Session;
using TitanControl.Views.Controls.Handle;

namespace TitanControl.ViewModels.Workspace.Handle
{
    public class HandleButtonModel : HandleControlModel<ButtonControlModel>
    {
        public string Color { get; set; } = "#4A5562";
        public string Legend { get; set; } = "Button";

        public HandleButtonModel(ButtonControlModel model, ISessionService service) : base(model, service)
        {

        }

        public override IHandleControl Copy()
        {
            return new HandleButtonModel(Model, SessionService)
            {
                Color = Color, 
                Legend = Legend  
            };
        }
    }
}
