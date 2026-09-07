using Avalonia.Controls;
using System.Drawing;
using System.Threading.Tasks;
using TitanControl.Models.Control;
using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Handle;
using TitanControl.WebAPI.Data;

namespace TitanControl.Views.Controls.Handle
{
    public interface IHandleControl : ISelectable
    {
        IControlModel Model { get; }

        HandleControlId ControlId { get; }
        Rectangle Location { get; set; }
        int TitanId { get; }
        HandleType HandleType { get; set; }
        HandleKeyProfile KeyProfile { get; set; }

        bool IsMoving { get; set; }

        Task ExecuteAsync();
        IHandleControl Copy();
    }

    public interface IHandleControl<TModel> : IHandleControl
        where TModel : IControlModel
    {
        new TModel Model { get; }
    }
}
