using Avalonia.Controls.Converters;
using TitanControl.Models.Control.Handle;
using TitanControl.Services.Session;

namespace TitanControl.ViewModels.Workspace.Controls.Handle
{
    public class HandleButtonModel : HandleControlModel<ButtonHandleModel>
    {
        public HandleButtonModel(ButtonHandleModel model, ISessionService service) : base(model, service)
        {

        }

        public override IHandleControl Copy()
        {
            return new HandleButtonModel(Model, SessionService)
            {
                HandleInformation = this.HandleInformation,
                HandleType = this.HandleType,
                KeyProfile = this.KeyProfile
            };
        }
    }
}
