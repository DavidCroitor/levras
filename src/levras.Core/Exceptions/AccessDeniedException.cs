namespace levras.Core.Exceptions;

public sealed class AccessDeniedException : WorkspaceIoException
{
    public AccessDeniedException(string path, Exception? innerException = null)
        : base(path, $"Access denied: '{path}'", innerException)
    {
    }
}