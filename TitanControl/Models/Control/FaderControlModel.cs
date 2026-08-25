using TitanControl.Services.Session;
using TitanControl.ViewModels.Controls.Handle;
using TitanControl.Views.Controls.Handle;

namespace TitanControl.Models.Control
{
    public class FaderControlModel : ControlModel
    {
        public override ControlId ControlId => ControlId.Fader;

        public override HandleFaderModel ToInstance(ISessionService service)
        {
            return new HandleFaderModel(this, service);
        }
    }
}
