using System.Drawing;
using System.Text.Json.Serialization;
using TitanControl.Services.Session;
using TitanControl.Views.Controls.Handle;
using TitanControl.WebAPI.Data;

namespace TitanControl.Models.Control
{
    public abstract class ControlModel : IControlModel
    {
        [JsonPropertyName("type")]
        public virtual ControlId ControlId { get; init; } = ControlId.None;

        [JsonPropertyName("location")]
        public Rectangle Location { get; set; }

        [JsonPropertyName("titanId")]
        public int TitanId { get; set; }

        [JsonPropertyName("handleType")]
        public HandleType HandleType { get; set; }

        [JsonPropertyName("keyProfile")]
        public KeyProfile KeyProfile { get; set; }

        public abstract ISaveable ToInstance(ISessionService service);

        public T ToInstance<T>(ISessionService service)
        {
            return (T)ToInstance(service);
        }
    }
}
