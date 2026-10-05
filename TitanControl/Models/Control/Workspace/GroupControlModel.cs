using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Models.Control.Handle;
using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Controls;
using TitanControl.ViewModels.Workspace.Controls.Handle;

namespace TitanControl.Models.Control.Workspace
{
    public class GroupControlModel : IControlModel
    {
        public ControlId ControlId => ControlId.Group;
        public Rectangle Location { get; set; }
        public List<IHandleModel> Controls { get; set; } = new List<IHandleModel>();

        public IWorkspaceControl ToInstance(ISessionService service)
        {
            return new GroupControl()
            {
                Location = this.Location,
                Controls = new System.Collections.ObjectModel.ObservableCollection<IHandleControl>(
                    this.Controls.Select(c => c.ToInstance<IHandleControl>(service))
                )
            };
        }

        public T ToInstance<T>(ISessionService service)
        {
            return (T)ToInstance(service);
        }
    }
}
