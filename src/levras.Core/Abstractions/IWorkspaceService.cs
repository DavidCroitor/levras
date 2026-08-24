using levras.Core.Models;

namespace levras.Core.Abstractions;

public interface IWorkspaceService
{
    string? CurrentWorkspacePath {get;}
    void OpenWorkspace(string folderPath);
    Task<IReadOnlyList<WorkspaceItem>> GetWorkspaceTreeAsync(CancellationToken cancellationToken = default);
    bool IsPathWithinWorkspace(string path);

}