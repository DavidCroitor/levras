namespace levras.Core.Exceptions;

public sealed class InvalidNodeNameException : WorkspaceIoException
{
    public InvalidNodeNameException(string path, Exception? innerException = null) 
    : base(path, $"{path} is invalid", innerException){}
}