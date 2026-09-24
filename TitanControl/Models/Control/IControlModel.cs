using System.Drawing;
using System.Text.Json.Serialization;
using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Handle;
using TitanControl.WebAPI.Data;

namespace TitanControl.Models.Control
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(ButtonControlModel), "button")]
    [JsonDerivedType(typeof(FaderControlModel), "fader")]
    public interface IControlModel : ISaveModel
    {
        HandleControlId ControlId { get; init; }
        Rectangle Location { get; set; }
        int TitanId { get; set; }
        HandleType HandleType { get; set; }
        HandleKeyProfile KeyProfile { get; set; }

        T ToInstance<T>(ISessionService service);
    }
}
