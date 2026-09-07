using System.Drawing;
using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Handle;
using TitanControl.WebAPI.Data;

namespace TitanControl.Models.Control
{
    public interface IControlModel : ISaveModel
    {
        HandleControlId ControlId { get; init; }
        Rectangle Location { get; set; }
        int TitanId { get; set; }
        HandleType HandleType { get; set; }
        HandleKeyProfile KeyProfile { get; set; }

        ISaveable ToInstance(ISessionService service);
    }
}
