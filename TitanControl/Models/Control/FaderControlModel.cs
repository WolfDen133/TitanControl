using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Handle;

namespace TitanControl.Models.Control
{
    public class FaderControlModel : ControlModel
    {
        public override HandleControlId ControlId => HandleControlId.Fader;

        public override HandleFaderModel ToInstance(ISessionService service)
        {
            return new HandleFaderModel(this, service);
        }
    }
}
