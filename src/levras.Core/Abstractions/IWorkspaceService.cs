using levras.Core.Domain;

namespace levras.Core.Abstractions;

public interface IWorkspaceService
{
    string? CurrentWorkspacePath {get;}
    void OpenWorkspace(string folderPath);
    Task<IReadOnlyList<WorkspaceItem>> GetWorkspaceTreeAsync(CancellationToken cancellationToken = default);
    bool IsPathWithinWorkspace(string path);
    Task<string> ReadFileAsync(
            string filePath,
            CancellationToken cancellationToken = default);
    Task<byte[]> ReadFileBytesAsync(
            string filePath, 
            CancellationToken cancellationToken = default);
    Task WriteFileAsync(
            string filePath,
            string content,
            CancellationToken cancellationToken = default);
    Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default);
    Task<WorkspaceItem> CreateFileAsync(
            string parentDirectoryPath, 
            string fileName, 
            CancellationToken cancellationToken = default);
    Task<WorkspaceItem> CreateFolderAsync(
            string parentDirectoryPath, 
            string folderName, 
            CancellationToken cancellationToken = default);
    Task<WorkspaceItem> MoveAsync(
            string sourcePath, 
            string destinationDirectoryPath, 
            CancellationToken cancellationToken = default);
    Task<WorkspaceItem> RenameAsync(
            string sourcePath, 
            string newName, 
            CancellationToken cancellationToken = default);
        
}