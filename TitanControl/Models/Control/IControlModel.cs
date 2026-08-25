using System.Drawing;
using TitanControl.Services.Session;
using TitanControl.Views.Controls.Handle;
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
