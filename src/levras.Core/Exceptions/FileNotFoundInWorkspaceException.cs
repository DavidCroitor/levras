namespace levras.Core.Exceptions;

public sealed class FileNotFoundInWorkspaceException : WorkspaceIoException
{
    public FileNotFoundInWorkspaceException(string path, Exception? innerException = null): base(path, $"File not found: '{NameOf(path)}'.", innerException){}
}