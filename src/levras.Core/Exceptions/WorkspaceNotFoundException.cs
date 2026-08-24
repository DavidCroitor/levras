namespace levras.Core.Exceptions;

public sealed class WorkspaceNotFoundException : WorkspaceIoException
{
    public WorkspaceNotFoundException(string path, Exception? innerException = null)
        : base(path, $"Workspace folder not found: '{path}'", innerException)
    {
    }
}