using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TitanControl.Disk.Converter
{
    public sealed class IPAddressArrayJsonConverter : JsonConverter<IPAddress>
    {
        public override IPAddress Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected an array for IPAddress.");

            byte[] addressBytes = new byte[4];
            
            for (int i = 0; i < addressBytes.Length; i++)
            {
                reader.Read();
                addressBytes[i] = reader.GetByte();
            }

            reader.Read();

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException(
                    "IPAddress must contain exactly 4 values.");

            return new IPAddress(addressBytes);
        }

        public override void Write(
            Utf8JsonWriter writer,
            IPAddress value,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.GetAddressBytes()[0]);
            writer.WriteNumberValue(value.GetAddressBytes()[1]);
            writer.WriteNumberValue(value.GetAddressBytes()[2]);
            writer.WriteNumberValue(value.GetAddressBytes()[3]);
            writer.WriteEndArray();
        }
    }
}
