using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Models.Control;
using TitanControl.Services.Command.Map;
using TitanControl.ViewModels.Workspace.Controls.Handle;

namespace TitanControl.Services.Command
{
    public interface ICommandService : IService
    {
        void RegisterMap(ControlId controlType, ICommandMap commandMap);

        ICommandMap Get(ControlId controlType);
    }
}
