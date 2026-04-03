namespace Notifications.Interfaces;

public interface ITemplateService
{
    Task<string> RenderAsync(string templateName, IDictionary<string, string> placeholders, CancellationToken cancellationToken = default);
}
