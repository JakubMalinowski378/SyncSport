using Microsoft.AspNetCore.Http;

namespace Shared.Extensions;

public static class FormFileExtensions
{
    public static IEnumerable<(Stream Stream, string ContentType, string FileName)> ToUploadStreams(this IFormFileCollection files)
    {
        if (files is null) yield break;

        foreach (var file in files)
        {
            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            var newFileName = $"{Guid.NewGuid()}{extension}";
            yield return (file.OpenReadStream(), file.ContentType, newFileName);
        }
    }
}
