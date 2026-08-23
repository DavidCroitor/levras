namespace levras.Core.Abstractions;

public interface IFileSystemService
{
    Task<string> ReadTextFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task WriteTextFileAsync(string filePath, string content, CancellationToken cancellationToken = default);
    Task DeleteTextFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string filePath);
    Task CreateDirectoryAsync(string path);
}