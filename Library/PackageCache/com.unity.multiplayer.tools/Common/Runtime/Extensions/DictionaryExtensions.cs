using System.Collections.Generic;

namespace Unity.Multiplayer.Tools.Common
{
    static class DictionaryExtensions
    {
        public static TValue GetValueOrCreateNew<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;

            value = new TValue();
            dictionary.Add(key, value);
            return value;
        }
    }
}
