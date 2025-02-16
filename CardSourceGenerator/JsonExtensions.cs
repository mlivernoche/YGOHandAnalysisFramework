using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CardSourceGenerator
{
    internal static class JsonExtensions
    {
        public static TReturn TryGetProperty<TReturn>(this JsonElement element, string propertyName, Func<JsonElement, TReturn> selector, TReturn def = default)
            where TReturn : notnull
        {
            if (element.TryGetProperty(propertyName, out var el))
            {
                return selector(el);
            }

            return def;
        }

        public static string TryGetProperty(this JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, el => el.GetString() ?? string.Empty, string.Empty);
        }
    }
}
