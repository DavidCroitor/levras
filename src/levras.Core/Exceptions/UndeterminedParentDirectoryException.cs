namespace levras.Core.Exceptions;

public sealed class UndeterminedParentDirectoryException : WorkspaceIoException
{
    public UndeterminedParentDirectoryException(string path, Exception? innerException = null)
    : base(path, $"Cannot determine parent directory of {path}" ,innerException){   }
}