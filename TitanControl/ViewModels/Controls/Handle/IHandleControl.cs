using Avalonia.Controls;
using System.Drawing;
using System.Threading.Tasks;
using TitanControl.Models.Control;
using TitanControl.Services.Command;
using TitanControl.Services.Command.Map;
using TitanControl.Services.Session;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Data.Model;
using HandleInformation = TitanControl.WebAPI.Data.Model.Handle;

namespace TitanControl.ViewModels.Workspace.Controls.Handle
{
    public interface IHandleControl : IWorkspaceControl
    {
        IControlModel Model { get; }
        int? TitanId { get; }
        HandleType HandleType { get; set; }
        HandleKeyProfile KeyProfile { get; set; }
        ICommandMap? CommandMap { get; set; }
        HandleInformation? HandleInformation { get; set; }

        Task ExecuteAsync(CommandAction action);
    }

    public interface IHandleControl<TModel> : IHandleControl
        where TModel : IControlModel
    {
        new TModel Model { get; }
    }
}
