using Scriban;
using System.Collections.Concurrent;

namespace Notifications;

internal sealed class TemplateService : ITemplateService
{
    private readonly ConcurrentDictionary<string, Template> _templateCache = new();

    public async Task<string> RenderAsync(string templateName, IDictionary<string, string> placeholders, CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(templateName, cancellationToken);
        return await template.RenderAsync(placeholders);
    }

    private async Task<Template> GetTemplateAsync(string templateName, CancellationToken cancellationToken)
    {
        if (_templateCache.TryGetValue(templateName, out var cachedTemplate))
        {
            return cachedTemplate;
        }

        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var templatePath = Path.Combine(basePath, "Templates", $"{templateName}.html");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"The template '{templateName}' could not be found at '{templatePath}'.");
        }

        var content = await File.ReadAllTextAsync(templatePath, cancellationToken);
        var template = Template.Parse(content);

        if (template.HasErrors)
        {
            throw new InvalidOperationException($"Error parsing template '{templateName}': {string.Join(", ", template.Messages)}");
        }

        _templateCache.TryAdd(templateName, template);

        return template;
    }
}
