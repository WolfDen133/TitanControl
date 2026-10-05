using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Models.Control;

namespace TitanControl.ViewModels.Workspace.Controls
{
    public interface IWorkspaceControl : ISelectable
    {
        ControlId ControlId { get; }
        Rectangle Location { get; set; }

        bool IsMoving { get; set; }

        IWorkspaceControl Copy();
    }
}
