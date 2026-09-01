namespace levras.Core.Exceptions;

public sealed class ByteReadingException : WorkspaceIoException
{
    public ByteReadingException(string path, Exception? innerException = null): base(path, $"Could not read: '{path}'.", innerException){}
}