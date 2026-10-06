using Avalonia.Controls.Converters;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows.Input;
using TitanControl.Logging;
using TitanControl.Models.Control.Handle;
using TitanControl.Services.Command.Map;
using TitanControl.Services.Session;

namespace TitanControl.ViewModels.Workspace.Controls.Handle
{
    public partial class HandleButtonModel : HandleControlModel<ButtonHandleModel>
    {
        public HandleButtonModel(ButtonHandleModel model) : base(model)
        { }

        public override IHandleControl Copy()
        {
            return new HandleButtonModel(Model)
            {
                HandleInformation = this.HandleInformation,
                CommandMap = CommandMap
            };
        }

        [RelayCommand]
        public async Task Press()
        {
            await CommandMap!.ExecuteAsync(KeyProfile, HandleInformation, Services.Command.CommandAction.ButtonDown);
        }

        [RelayCommand]
        public async Task Release()
        {
            await CommandMap!.ExecuteAsync(KeyProfile, HandleInformation, Services.Command.CommandAction.ButtonUp);
        }
    }
}
