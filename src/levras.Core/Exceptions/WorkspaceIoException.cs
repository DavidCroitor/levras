namespace levras.Core.Exceptions;

/// <summary>
/// Base exception for all file/folder operation failures within a workspace context.
/// Infrastructure implementations catch framework-specific exceptions (IOException,
/// UnauthorizedAccessException, etc.) and rethrow as one of these, so Presentation
/// never needs to know about System.IO exception types.
/// </summary>
public abstract class WorkspaceIoException : Exception
{
    public string Path { get; }

    protected WorkspaceIoException(string path, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Path = path;
    }
}