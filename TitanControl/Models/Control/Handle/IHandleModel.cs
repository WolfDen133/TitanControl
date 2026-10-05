using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.ViewModels.Workspace.Controls.Handle;
using TitanControl.WebAPI.Data;

namespace TitanControl.Models.Control.Handle
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(ButtonHandleModel), "button")]
    [JsonDerivedType(typeof(FaderHandleModel), "fader")]
    public interface IHandleModel : IControlModel
    {
        int TitanId { get; set; }
        HandleType HandleType { get; set; }
        HandleKeyProfile KeyProfile { get; set; }
    }
}
