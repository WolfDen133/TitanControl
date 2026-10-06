using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Controls.Handle;
using TitanControl.Views.Controls.Toolbar.Button;
using TitanControl.WebAPI;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Data.Model;
using ApiHandle = TitanControl.WebAPI.Data.Model.Handle;

namespace TitanControl.Services.Command.Map
{
    public class ButtonCommandMap : ICommandMap
    {
        private ISessionService _service;
        private Titan? _session => _service.CurrentSession?.Api;

        private readonly Dictionary<(HandleType, HandleKeyProfile), Dictionary<CommandAction, Func<HandleReference, Task>>> _commandMap = new();

        public ButtonCommandMap(ISessionService service)
        {
            _service = service;
        }

        public async Task Initialize()
        {
            RegisterMaps();
        }

        public void RegisterMaps()
        {
            _commandMap.Add((HandleType.None, HandleKeyProfile.None), new()
            {
                [CommandAction.ButtonDown] = async (handleReference) => Log.Debug("Button down"),
                [CommandAction.ButtonUp] = async (handleReference) => Log.Debug("Button up"),
            });

            _commandMap.Add((HandleType.Cue, HandleKeyProfile.Flash), new()
            {
                [CommandAction.ButtonDown] = async (handleReference) => _session?.Playbacks.FlashDown(handleReference),
                [CommandAction.ButtonUp] = async (handleReference) => _session?.Playbacks.FlashUp(handleReference),
            });

            _commandMap.Add((HandleType.Cue, HandleKeyProfile.TimedFlash), new()
            {
                [CommandAction.ButtonDown] = async (handleReference) => _session?.Playbacks.TimedFlashDown(handleReference),
                [CommandAction.ButtonUp] = async (handleReference) => _session?.Playbacks.TimedFlashUp(handleReference),
            });
        }

        public Task ExecuteAsync(HandleKeyProfile profile, ApiHandle? handle, CommandAction action)
        {
            if (_session is null)
                return Task.CompletedTask;

            if (!_commandMap.TryGetValue((handle?.Type ?? HandleType.None, profile), out var command))
            {
                Log.Warning($"No command found for the { profile } profile and { handle?.Type ?? HandleType.None } handle.", "ButtonCommandMap");

                return Task.CompletedTask;
            }

            return command[action](HandleReference.FromTitanId(handle?.TitanId ?? -1));
        }
    }
}
