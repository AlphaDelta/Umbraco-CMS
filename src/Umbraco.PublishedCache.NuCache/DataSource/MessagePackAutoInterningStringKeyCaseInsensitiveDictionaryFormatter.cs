using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource
{
    /// <summary>
    /// A MessagePack formatter (deserializer) for a string key dictionary that uses OrdinalIgnoreCase for the key string comparison.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public sealed class MessagePackAutoInterningStringKeyCaseInsensitiveDictionaryFormatter<TValue> : DictionaryFormatterBase<string, TValue, Dictionary<string, TValue>, Dictionary<string, TValue>.Enumerator, Dictionary<string, TValue>>
    {
        protected override void Add(Dictionary<string, TValue> collection, int index, string key, TValue value, MessagePackSerializerOptions options)
        {
            string.Intern(key);
            collection.Add(key, value);
        }

        protected override Dictionary<string, TValue> Complete(Dictionary<string, TValue> intermediateCollection) => intermediateCollection;

        protected override Dictionary<string, TValue>.Enumerator GetSourceEnumerator(Dictionary<string, TValue> source) => source.GetEnumerator();

        protected override Dictionary<string, TValue> Create(int count, MessagePackSerializerOptions options) => new Dictionary<string, TValue>(count, StringComparer.OrdinalIgnoreCase);
    }
}
