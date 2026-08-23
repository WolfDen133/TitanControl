using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.Services.Session;
using TitanControl.WebAPI.Data;

namespace TitanControl.Models.Control
{
    public interface IControlModel : ISaveModel
    {
        ControlId ControlId { get; init; }
        Rectangle Location { get; set; }
        int TitanId { get; set; }
        HandleType HandleType { get; set; }
        KeyProfile KeyProfile { get; set; }

        ISaveable ToInstance(ISessionService service);
    }
}
