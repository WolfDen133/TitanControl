using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TitanControl.Disk.Converter
{
    public sealed class SizeArrayJsonConverter : JsonConverter<Size>
    {
        public override Size Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected an array for Size.");

            reader.Read();
            int width = reader.GetInt32();

            reader.Read();
            int height = reader.GetInt32();

            reader.Read();

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException("Size must contain exactly 2 values.");

            return new Size(width, height);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Size value,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Width);
            writer.WriteNumberValue(value.Height);
            writer.WriteEndArray();
        }
    }
}
