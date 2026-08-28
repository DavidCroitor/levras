namespace levras.Core.Abstractions;

public interface IFileSystemService
{
    Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string filePath);
    Task CreateDirectoryAsync(string path);
}