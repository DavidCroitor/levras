namespace levras.Core.Exceptions;

/// <summary>
/// Thrown when an operation targets a path outside the currently opened workspace.
/// This is the enforcement mechanism for the "all actions scoped to workspace" rule.
/// </summary>
public sealed class PathOutsideWorkspaceException : WorkspaceIoException
{
    public PathOutsideWorkspaceException(string path, Exception? innerException = null)
        : base(path, $"Path is outside the current workspace: '{path}'", innerException)
    {
    }
}