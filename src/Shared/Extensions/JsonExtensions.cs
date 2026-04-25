using System.Text.Json;

namespace Shared.Extensions;

public static class JsonExtensions
{
    public static T? DeserializeJson<T>(this string? json, JsonSerializerOptions options)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(json, options);
    }
}
