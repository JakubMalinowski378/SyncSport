using Facilities.Domain.Utilities;

namespace Facilities.Application.Facilities.Common;

public static class UniqueSlugProvider
{
    public static async Task<string> GenerateAsync(string source, Func<string, Task<bool>> slugExistsAsync)
    {
        var baseSlug = SlugGenerator.Generate(source);
        var candidate = baseSlug;

        if (!await slugExistsAsync(candidate))
        {
            return candidate;
        }

        var suffix = 2;
        while (await slugExistsAsync($"{baseSlug}-{suffix}"))
        {
            suffix++;
        }

        return $"{baseSlug}-{suffix}";
    }
}
