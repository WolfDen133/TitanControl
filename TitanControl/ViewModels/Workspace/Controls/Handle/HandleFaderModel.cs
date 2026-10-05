using TitanControl.Models.Control.Handle;
using TitanControl.Services.Session;

namespace TitanControl.ViewModels.Workspace.Controls.Handle
{
    public class HandleFaderModel : HandleControlModel<FaderHandleModel>
    {
        public HandleFaderModel(FaderHandleModel model, ISessionService service) : base(model, service)
        { }

        public override IHandleControl Copy()
        {
            return new HandleFaderModel(Model, SessionService);
        }
    }
}
