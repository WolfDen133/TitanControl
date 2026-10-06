using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Models.Control;
using TitanControl.Services.Command.Map;
using TitanControl.Services.Session;

namespace TitanControl.Services.Command
{
    public class CommandService : ICommandService
    {
        private Dictionary<ControlId, ICommandMap> _map = new Dictionary<ControlId, ICommandMap>();
        private ISessionService _service;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CommandService(ISessionService service)
        {
            _service = service;
        }

        public async Task InitializeAsync()
        {
            RegisterMap(ControlId.Button, new ButtonCommandMap(_service));
        }

        public void Dispose()
        {
            _map.Clear();
        }

        public ICommandMap Get(ControlId controlType)
        {
            if (_map.TryGetValue(controlType, out ICommandMap? commandMap))
            {
                return commandMap;
            }

            throw new ArgumentException($"No command map registered for control type {controlType}");
        }

        public void RegisterMap(ControlId controlType, ICommandMap commandMap)
        {
            if (_map.ContainsKey(controlType))
            {
                throw new ArgumentException($"A command map is already registered for control type {controlType}");
            }

            commandMap.Initialize();
            _map[controlType] = commandMap;
        }
    }
}
