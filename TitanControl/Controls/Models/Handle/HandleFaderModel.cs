using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Models.Control;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;

namespace TitanControl.Controls.Models.Handle
{
    public class HandleFaderModel : HandleControlModel<ControlModel>
    {
        public HandleFaderModel(FaderControlModel model, ISessionService service) : base(model, service)
        { }

        
    }
}
