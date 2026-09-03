using levras.Core.Exceptions;

namespace levras.Core.Domain;

public static class WorkspaceFileNameValidator
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();

    public static void EnsureValid(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNodeNameException("Name cannot be empty");
        }
        if(name.IndexOfAny(InvalidChars) >= 0)
        {
            throw new InvalidNodeNameException($"'{name}' contains invalid characters");
        }
    }

}