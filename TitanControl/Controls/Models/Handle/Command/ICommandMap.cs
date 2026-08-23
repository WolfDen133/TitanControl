using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.Controls.Models;
using TitanControl.WebAPI.Data;

namespace TitanControl.Controls.Models.Handle.Command
{
    public interface ICommandMap<TitanControlModel>
    {
        Task ExecuteAsync(
            KeyProfile profile,
            HandleType handle,
            TitanControlModel control);
    }
}
