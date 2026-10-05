using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Controls.Handle;

namespace TitanControl.Models.Control.Handle
{
    public class ButtonHandleModel : HandleModel
    {
        public override ControlId ControlId => ControlId.Button;

        public override HandleButtonModel ToInstance(ISessionService service)
        {
            return new HandleButtonModel(this, service);
        }
    }
}
