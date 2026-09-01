using System.Net.Http.Headers;
using levras.Core.Abstractions;
using levras.Core.Domain;
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

    public Task DeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.Delete(path, recursive: true);
            return Task.CompletedTask;
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new AccessDeniedException(path, ex);
        }
        catch (IOException ex)
        {
            throw new WorkspaceIoFailureException(path, ex);
        }
        
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
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

    public Task<bool> DirectoryExistsAsync(string path) => Task.FromResult(Directory.Exists(path));

    public Task<bool> FileExistsAsync(string filePath) => Task.FromResult(File.Exists(filePath));

    public Task MoveDirectoryAsync(string sourceDirectoryPath, string destinationDirectoryPath, CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.Move(sourceDirectoryPath, destinationDirectoryPath);
            return Task.CompletedTask;
        }
        catch (IOException ex)
        {
            throw new InvalidMoveException(sourceDirectoryPath, destinationDirectoryPath, ex);
        }
    }

    public Task MoveFileAsync(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            File.Move(sourceFilePath, destinationFilePath);
            return Task.CompletedTask;
        }
        catch (IOException ex)
        {
            throw new InvalidMoveException(sourceFilePath, destinationFilePath, ex);
        }
    }

    public async Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
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

    public async Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
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