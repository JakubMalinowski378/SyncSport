namespace Storage;

public interface IImageStorageService
{
    Task<string> AddAsync(Stream content, string contentType, string fileName, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> AddRangeAsync(IEnumerable<(Stream content, string contentType, string fileName)> files, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
}