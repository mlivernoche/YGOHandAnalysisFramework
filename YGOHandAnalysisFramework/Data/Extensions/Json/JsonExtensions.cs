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
    public static Result<TValue, Exception> TryParseValue<TValue>(Stream source)
    {
        try
        {
            var parsedValue = JsonSerializer.Deserialize<TValue>(source);

            if (parsedValue is not null)
            {
                return new(parsedValue);
            }
        }
        catch (ArgumentNullException argumentNullException)
        {
            return new(argumentNullException);
        }
        catch (JsonException jsonException)
        {
            return new(jsonException);
        }
        catch (NotSupportedException notSupportedException)
        {
            return new(notSupportedException);
        }

        throw new NotImplementedException();
    }

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
