using levras.Core.Models;

namespace levras.Core.Abstractions;

public interface IWorkspaceSystem
{
    string? CurrentWorkspacePath {get;}
    Task<IReadOnlyList<WorkspaceItem>> GetWorkspaceTreeAsync(CancellationToken cancellationToken = default);
    bool IsPathWithinWorkspace(string path);

}