using TitanControl.Services.Session;
using TitanControl.ViewModels.Controls.Handle;
using TitanControl.Views.Controls.Handle;

namespace TitanControl.Models.Control
{
    public class ButtonControlModel : ControlModel
    {
        public override ControlId ControlId => ControlId.Button;

        public override HandleButtonModel ToInstance(ISessionService service)
        {
            return new HandleButtonModel(this, service);
        }
    }
}
