using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis
{
    /// <summary>
    /// JsonConverter to allow serializing colors (and types that contain them like gradients)
    /// </summary>
    /// <remarks>
    /// Without an explicit converter to save and load colors, the following error
    /// is thrown when attempting to serialize colors or gradients:
    /// "JsonSerializationException: Self referencing loop detected for property 'linear' with type 'UnityEngine.Color'."
    /// </remarks>
    class JsonColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(
            JsonWriter writer,
            Color value,
            JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.r);
            writer.WriteValue(value.g);
            writer.WriteValue(value.b);
            writer.WriteValue(value.a);
            writer.WriteEndArray();
        }

        public override Color ReadJson(
            JsonReader reader,
            Type objectType,
            Color existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
            {
                return new();
            }

            reader.Read();

            var color = new Color();
            color.r = ReadFloat(reader);
            color.g = ReadFloat(reader);
            color.b = ReadFloat(reader);
            color.a = ReadFloat(reader);

            return color;
        }

        static float ReadFloat(JsonReader reader)
        {
            var value = reader.TokenType == JsonToken.Float
                ? Convert.ToSingle(reader.Value)
                : default;
            reader.Read();
            return value;
        }
    }
}
