using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TitanControl.WebAPI.Data.Conversion;

namespace TitanControl.WebAPI.Data.Model
{
    public class Handle
    {
        /// <summary>
        /// Gets or sets the titan ID for the item linked to this handle..
        /// </summary>
        public int TitanId { get; set; }

        [JsonPropertyName("userNumber")]
        public HandleUserNumber UserNumber { get; set; } = null!;

        [JsonPropertyName("properties")]
        public HandleProperty[] Properties { get; set; } = new HandleProperty[0];

        /// <summary>
        /// Gets or sets the type of this handle such a fixtureHandle, cueListHandle etc.
        /// </summary>
        [JsonConverter(typeof(HandleTypeConverter))]
        public HandleType Type { get; set; }

        /// <summary>
        /// Gets or sets the legend of the item linked to this handle.
        /// </summary>
        public string Legend { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the notes related to this handle.
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the halo colour for this handle in the form #AARRGGBB
        /// </summary>
        /// <remarks>
        /// The halo colour is represented as a hex number in the form #AARRGGBB with Alpha, Red, Green and Blue components.
        /// 
        /// Please take note that CSS colours are in the form #RRGGBBAA and so the halo string
        /// will require converting when used with CSS.
        /// </remarks>
        public string Halo { get; set; } = string.Empty;

        /// <summary>
        /// Gets the icon for this handle.
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Gets whether this handle has been selected.
        /// </summary>
        /// <remarks>
        /// This will indicate whether the handle is selected by WebAPI not nesaserily by the console.
        /// </remarks>
        public bool Selected { get; set; }

        /// <summary>
        /// Gets whether this handle is considered active such as with a playback whether it is loaded.
        /// </summary>
        public bool Active { get; set; }

        [JsonPropertyName("Links")]
        public string[] Links { get; set; } = new string[0];

    }

    public class HandleUserNumber
    {
        [JsonPropertyName("hashCode")]
        public int Number { get; set; }

        [JsonPropertyName("userNumbers")]
        public int[] UserNumbers { get; set; } = new int[0];
    }

    public class HandleProperty
    {
        [JsonPropertyName("Key")]
        public string Key { get; set; } = string.Empty;
        [JsonPropertyName("Value")]
        public string Value { get; set; } = String.Empty;
    }
}
