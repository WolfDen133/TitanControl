using System.Threading.Tasks;
using TitanControl.ViewModels.Workspace.Controls.Handle;
using TitanControl.WebAPI.Data;
using ApiHandle = TitanControl.WebAPI.Data.Model.Handle;

namespace TitanControl.Services.Command.Map
{
    public interface ICommandMap
    {
        Task Initialize();

        Task ExecuteAsync(
            HandleKeyProfile profile,
            ApiHandle? handle,
            CommandAction action);
    }
}
