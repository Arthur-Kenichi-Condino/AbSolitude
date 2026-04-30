using System.Collections.Generic;
using Newtonsoft.Json;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    static class JsonSerialization
    {
        static readonly JsonSerializerSettings k_Settings = new JsonSerializerSettings
        {
            // IgnoreAndPopulate explanation from the documentation:
            // > Ignore members where the member value is the same as the member's default
            // > value when serializing objects and set members to their default value when
            // > deserializing.
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            Converters = new List<JsonConverter>
            {
                new JsonColorConverter(),
            }
        };

        public static string ToJson<T>(T value)
        {
            return JsonConvert.SerializeObject(value, settings: k_Settings);
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, settings: k_Settings);
        }
    }
}
