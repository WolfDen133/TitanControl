using TitanControl.Models.Control;
using TitanControl.Services.Session;

namespace TitanControl.ViewModels.Controls.Handle
{
    public class HandleFaderModel : HandleControlModel<ControlModel>
    {
        public HandleFaderModel(FaderControlModel model, ISessionService service) : base(model, service)
        { }


    }
}
