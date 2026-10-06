using System.Drawing;
using System.Text.Json.Serialization;
using TitanControl.Models.Control.Handle;
using TitanControl.Models.Control.Workspace;
using TitanControl.Services.Session;
using TitanControl.WebAPI.Data;

namespace TitanControl.Models.Control
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(GroupControlModel), "group")]
    [JsonDerivedType(typeof(ButtonHandleModel), "button")]
    [JsonDerivedType(typeof(FaderHandleModel), "fader")]
    public interface IControlModel : ISaveModel
    {
        ControlId ControlId { get; }
        Rectangle Location { get; set; }

        T ToInstance<T>();
    }
}
