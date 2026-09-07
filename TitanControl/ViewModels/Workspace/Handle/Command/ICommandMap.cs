using System.Threading.Tasks;
using TitanControl.WebAPI.Data;

namespace TitanControl.ViewModels.Workspace.Handle.Command
{
    public interface ICommandMap<TitanControlModel>
    {
        Task ExecuteAsync(
            HandleKeyProfile profile,
            HandleType handle,
            TitanControlModel control);
    }
}
