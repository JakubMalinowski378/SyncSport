using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Shared.FluentValidation;

public static class FormFileValidationExtensions
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    public static IRuleBuilderOptionsConditions<T, IFormFileCollection?> ValidateImageFormFiles<T>(
        this IRuleBuilder<T, IFormFileCollection?> ruleBuilder)
    {
        return ruleBuilder
            .Custom((files, context) =>
            {
                if (files is null || !files.Any())
                {
                    return;
                }

                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!AllowedImageExtensions.Contains(extension))
                    {
                        context.AddFailure(
                            $"Invalid image format for file '{file.FileName}'. " +
                            $"Allowed extensions are: {string.Join(", ", AllowedImageExtensions)}");
                    }
                }
            });
    }
}
