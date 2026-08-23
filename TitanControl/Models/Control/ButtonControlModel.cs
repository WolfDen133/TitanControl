using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.Controls.Models.Handle;
using TitanControl.Services.Session;

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
