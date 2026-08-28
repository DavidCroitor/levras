namespace levras.Core.Exceptions;

/// <summary>
/// Catch-all for I/O failures that don't map to a more specific case
/// (disk full, device error, unexpected IOException, etc.).
/// </summary>
public sealed class WorkspaceIoFailureException : WorkspaceIoException
{
    public WorkspaceIoFailureException(string path, Exception innerException)
        : base(path, $"An I/O error occurred while accessing '{path}'", innerException)
    {
    }
}