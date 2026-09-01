namespace levras.Core.Exceptions;

public sealed class InvalidMoveException : WorkspaceIoException
{
    public InvalidMoveException(string path, Exception? innerException = null) 
    : base(path, $"Cannot move to {path}", innerException){}
}