using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TitanControl.WebAPI.Data.Conversion
{
    internal class HandleTypeConverter : JsonConverter<HandleType>
    {
        public const string Fixture =       "fixtureHandle";
        public const string Group =         "groupHandle";
        public const string Cue =           "cueHandle";
        public const string CueList =       "cueListHandle";
        public const string Chase =         "chaseHandle";
        public const string Track =         "trackHandle";
        public const string Palette =       "paletteHandle";
        public const string Macro =         "macroHandle";
        public const string Master =        "masterHandle";
        public const string Scene =         "abMasterHandle";
        public const string Rate =          "rateMasterHandle";
        public const string PlaybackGroup = "playbackGroupHandle";

        public override HandleType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? rawType = reader.GetString();
            reader.Read();

            if (rawType == null) return HandleType.None;

            return ToEnum(rawType);
        }

        public override void Write(Utf8JsonWriter writer, HandleType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(ToRawString(value));
        }

        private static HandleType ToEnum(string rawString)
        {
            return rawString switch
            {
                Fixture       => HandleType.Fixture,
                Group         => HandleType.Group,
                Cue           => HandleType.Cue,
                CueList       => HandleType.CueList,
                Chase         => HandleType.Chase,
                Track         => HandleType.Track,
                Palette       => HandleType.Palette,
                Macro         => HandleType.Macro,
                Master        => HandleType.Master,
                Scene         => HandleType.Scene,
                Rate          => HandleType.Rate,
                PlaybackGroup => HandleType.PlaybackGroup,
                _             => HandleType.None
            };
        }

        private static string ToRawString(HandleType handleType)
        {
            return handleType switch
            {
                  HandleType.Fixture       => Fixture,      
                  HandleType.Group         => Group,        
                  HandleType.Cue           => Cue,          
                  HandleType.CueList       => CueList,      
                  HandleType.Chase         => Chase,        
                  HandleType.Track         => Track,        
                  HandleType.Palette       => Palette,      
                  HandleType.Macro         => Macro,        
                  HandleType.Master        => Master,       
                  HandleType.Scene         => Scene,        
                  HandleType.Rate          => Rate,         
                  HandleType.PlaybackGroup => PlaybackGroup,
                  _                        => string.Empty
            };

        }
    }
}
