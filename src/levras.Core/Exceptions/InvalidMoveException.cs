namespace levras.Core.Exceptions;

public sealed class InvalidMoveException : WorkspaceIoException
{
    public InvalidMoveException(string sourcePath, string destinationPath, Exception? innerException = null) 
    : base(sourcePath, $"Cannot move to {destinationPath}", innerException){}
}