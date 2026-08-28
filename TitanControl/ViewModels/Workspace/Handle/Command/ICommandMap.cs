using System.Threading.Tasks;
using TitanControl.Views.Controls.Handle;
using TitanControl.WebAPI.Data;

namespace TitanControl.ViewModels.Workspace.Handle.Command
{
    public interface ICommandMap<TitanControlModel>
    {
        Task ExecuteAsync(
            KeyProfile profile,
            HandleType handle,
            TitanControlModel control);
    }
}
