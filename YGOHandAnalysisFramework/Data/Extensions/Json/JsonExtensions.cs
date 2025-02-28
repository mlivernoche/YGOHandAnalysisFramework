using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YGOHandAnalysisFramework.Features.Console.Json;

namespace YGOHandAnalysisFramework.Data.Extensions.Json;

public static class JsonExtensions
{
    public static bool TryParseValue<TValue>(string json, [NotNullWhen(true)] out TValue? value)
    {
        try
        {
            var parsedValue = JsonSerializer.Deserialize<TValue>(json);

            if(parsedValue is not null)
            {
                value = parsedValue;
                return true;
            }
        }
        catch(JsonException)
        {
            value = default;
            return false;
        }

        value = default;
        return false;
    }
}
