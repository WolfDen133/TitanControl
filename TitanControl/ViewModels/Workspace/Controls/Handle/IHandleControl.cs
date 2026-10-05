using Avalonia.Controls;
using System.Drawing;
using System.Threading.Tasks;
using TitanControl.Models.Control;
using TitanControl.Services.Session;
using TitanControl.WebAPI.Data;

namespace TitanControl.ViewModels.Workspace.Controls.Handle
{
    public interface IHandleControl : IWorkspaceControl
    {
        IControlModel Model { get; }
        int? TitanId { get; }
        HandleType HandleType { get; set; }
        HandleKeyProfile KeyProfile { get; set; }

        Task ExecuteAsync();
    }

    public interface IHandleControl<TModel> : IHandleControl
        where TModel : IControlModel
    {
        new TModel Model { get; }
    }
}
