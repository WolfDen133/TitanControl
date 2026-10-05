using Avalonia;
using System.Drawing;
using System.Text.Json.Serialization;
using TitanControl.Disk.Converter;
using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Controls.Handle;
using TitanControl.WebAPI.Data;

namespace TitanControl.Models.Control.Handle
{
    public abstract class HandleModel : IHandleModel
    {
        [JsonIgnore]
        public virtual ControlId ControlId { get; init; } = ControlId.None;

        [JsonPropertyName("location")]
        [JsonConverter(typeof(RectangleArrayJsonConverter))]
        public Rectangle Location { get; set; }

        [JsonPropertyName("titanId")]
        public int TitanId { get; set; }

        [JsonPropertyName("handleType")]
        public HandleType HandleType { get; set; }

        [JsonPropertyName("keyProfile")]
        public HandleKeyProfile KeyProfile { get; set; }

        public abstract ISaveable ToInstance(ISessionService service);

        public T ToInstance<T>(ISessionService service)
        {
            return (T)ToInstance(service);
        }
    }
}
