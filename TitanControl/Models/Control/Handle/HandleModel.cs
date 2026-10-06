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
        public int TitanId { get; set; } = -1;

        [JsonPropertyName("handleType")]
        public HandleType HandleType { get; set; } = HandleType.None;

        [JsonPropertyName("keyProfile")]
        public HandleKeyProfile KeyProfile { get; set; } = HandleKeyProfile.Flash;

        public abstract ISaveable ToInstance();

        public T ToInstance<T>()
        {
            return (T)ToInstance();
        }
    }
}
