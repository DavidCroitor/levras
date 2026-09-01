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
    Task<bool> DirectoryExistsAsync(string path);
    Task MoveFileAsync(
            string sourceFilePath, 
            string destinationFilePath, 
            CancellationToken cancellationToken = default);
    Task MoveDirectoryAsync(
            string sourceDirectoryPath, 
            string destinationDirectoryPath, 
            CancellationToken cancellationToken = default);

}