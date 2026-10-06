using TitanControl.Models.Control.Handle;
using TitanControl.Services.Command.Map;
using TitanControl.Services.Session;

namespace TitanControl.ViewModels.Workspace.Controls.Handle
{
    public class HandleFaderModel : HandleControlModel<FaderHandleModel>
    {
        public HandleFaderModel(FaderHandleModel model) : base(model)
        { }

        public override IHandleControl Copy()
        {
            return new HandleFaderModel(Model);
        }
    }
}
