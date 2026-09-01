using levras.Core.Domain;

namespace levras.Core.Abstractions;

public interface IFileSystemService
{
    Task<string> ReadFileAsync(
            string filePath, 
            CancellationToken cancellationToken = default);
    Task WriteFileAsync(
            string filePath, 
            string content, 
            CancellationToken cancellationToken = default);
    Task DeleteFileAsync(
            string filePath, CancellationToken 
            cancellationToken = default);
    Task<bool> FileExistsAsync(string filePath);
    Task<WorkspaceItem> CreateDirectoryAsync(
            string path,
            CancellationToken cancellationToken = default);
    Task<WorkspaceItem> CreateFileAsync(
            string filePath, 
            CancellationToken cancellationToken = default);
    Task<WorkspaceItem> MoveAsync(
            string sourcePath,
            string destinationPath,
            CancellationToken cancellationToken = default);
    Task<WorkspaceItem> RenameAsync(
            string sourcePath,
            string newName,
            CancellationToken cancellationToken = default);

}