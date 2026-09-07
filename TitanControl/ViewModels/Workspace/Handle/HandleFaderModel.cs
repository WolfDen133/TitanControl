using TitanControl.Models.Control;
using TitanControl.Services.Session;
using TitanControl.Views.Controls.Handle;

namespace TitanControl.ViewModels.Workspace.Handle
{
    public class HandleFaderModel : HandleControlModel<FaderControlModel>
    {
        public HandleFaderModel(FaderControlModel model, ISessionService service) : base(model, service)
        { }

        public override IHandleControl Copy()
        {
            return new HandleFaderModel(Model, SessionService);
        }
    }
}
