namespace levras.Core.Exceptions;

public sealed class NodeAlreadyExistsException : WorkspaceIoException
{
    public NodeAlreadyExistsException(string path, Exception? innerException = null)
    : base(path, $"Item already exists with name: {NameOf(path)}", innerException){}
}