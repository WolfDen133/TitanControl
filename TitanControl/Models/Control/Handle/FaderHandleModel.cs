using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Controls.Handle;

namespace TitanControl.Models.Control.Handle
{
    public class FaderHandleModel : HandleModel
    {
        public override ControlId ControlId => ControlId.Fader;

        public override HandleFaderModel ToInstance()
        {
            return new HandleFaderModel(this);
        }
    }
}
