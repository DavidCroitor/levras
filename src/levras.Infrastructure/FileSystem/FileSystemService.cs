using System.Net.Http.Headers;
using levras.Core.Abstractions;
using levras.Core.Exceptions;

namespace levras.Infrastructure.FileSystem;

public sealed class FileSystemService : IFileSystemService
{
    public Task CreateDirectoryAsync(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            return Task.CompletedTask;
        }
        catch(UnauthorizedAccessException ex)
        {
            throw new AccessDeniedException(path, ex);
        }
        catch(IOException ex)
        {
            throw new WorkspaceIoFailureException(path, ex);
        }
    }

    public Task DeleteTextFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if(!File.Exists(filePath))
            {
                throw new FileNotFoundInWorkspaceException(filePath);
            }
            File.Delete(filePath);
            return Task.CompletedTask;
        }
        catch(UnauthorizedAccessException ex)
        {
            throw new AccessDeniedException(filePath, ex);
        }
        catch(IOException ex)
        {
            throw new WorkspaceIoFailureException(filePath, ex);
        }
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        return Task.FromResult(File.Exists(filePath));
    }

    public async Task<string> ReadTextFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            return await File.ReadAllTextAsync(filePath, cancellationToken);
        }
        catch(FileNotFoundException ex)
        {
            throw new FileNotFoundInWorkspaceException(filePath, ex);
        }
        catch(UnauthorizedAccessException ex)
        {
            throw new AccessDeniedException(filePath, ex);
        }
        catch(IOException ex)
        {
            throw new WorkspaceIoFailureException(filePath, ex);
        }
    }

    public async Task WriteTextFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        try
        {
            await File.WriteAllTextAsync(filePath, content, cancellationToken);
        }
        catch(UnauthorizedAccessException ex)
        {
            throw new AccessDeniedException(filePath, ex);
        }
        catch(DirectoryNotFoundException ex)
        {
            throw new FileNotFoundInWorkspaceException(filePath, ex);
        }
        catch(IOException ex)
        {
            throw new WorkspaceIoFailureException(filePath, ex);
        }
    }
}