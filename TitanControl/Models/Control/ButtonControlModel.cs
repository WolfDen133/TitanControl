using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Handle;

namespace TitanControl.Models.Control
{
    public class ButtonControlModel : ControlModel
    {
        public override HandleControlId ControlId => HandleControlId.Button;

        public override HandleButtonModel ToInstance(ISessionService service)
        {
            return new HandleButtonModel(this, service);
        }
    }
}
