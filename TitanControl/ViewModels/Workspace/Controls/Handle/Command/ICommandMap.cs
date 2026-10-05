using System.Threading.Tasks;
using TitanControl.ViewModels.Workspace.Controls.Handle;
using TitanControl.WebAPI.Data;

namespace TitanControl.ViewModels.Workspace.Controls.Handle.Command
{
    public interface ICommandMap<TitanControlModel>
    {
        Task ExecuteAsync(
            HandleKeyProfile profile,
            HandleType handle,
            TitanControlModel control);
    }
}
