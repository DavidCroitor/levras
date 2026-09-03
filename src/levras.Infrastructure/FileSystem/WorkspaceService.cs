using levras.Core.Abstractions;
using levras.Core.Exceptions;
using levras.Core.Domain;

namespace levras.Infrastructure.FileSystem;

public sealed class WorkspaceService : IWorkspaceService
{
    private readonly IFileSystemService _fileSystemService;
    public WorkspaceService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }
    public string? CurrentWorkspacePath {get; private set;}
    private static readonly HashSet<string> IgnoredDirectoryNames = new(StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<WorkspaceItem>> GetWorkspaceTreeAsync(CancellationToken cancellationToken = default)
    {
        if(CurrentWorkspacePath is null)
        {
            throw new InvalidOperationException(
                "No workspace is currently open. Call OpenWorkspace first!"
            );
        }

        return await Task.Run(
            () => BuildTree(CurrentWorkspacePath, cancellationToken), cancellationToken
        );
    }

    public bool IsPathWithinWorkspace(string path)
    {
        if(CurrentWorkspacePath is null)
        {
            return false;
        }

        var fullPath = Path.GetFullPath(path);
        var workspaceRoot = Path.GetFullPath(CurrentWorkspacePath);

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return fullPath.Equals(workspaceRoot, comparison) || fullPath.StartsWith(workspaceRoot + Path.DirectorySeparatorChar, comparison);
    }

    public void OpenWorkspace(string folderPath)
    {
        var fullPath = Path.GetFullPath(folderPath);
        if(!Directory.Exists(fullPath))
        {
            throw new WorkspaceNotFoundException(folderPath);
        }
        CurrentWorkspacePath = fullPath;
    }

    public async Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if(!IsPathWithinWorkspace(filePath))
        {
            throw new PathOutsideWorkspaceException(filePath);
        }
        
        return await _fileSystemService.ReadFileAsync(filePath, cancellationToken);
    }

    public async Task<byte[]> ReadFileBytesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if(!IsPathWithinWorkspace(filePath))
        {
            throw new PathOutsideWorkspaceException(filePath);
        }
        return await _fileSystemService.ReadFileBytesAsync(filePath, cancellationToken);
    }
    public async Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        if(!IsPathWithinWorkspace(filePath))
        {
            throw new PathOutsideWorkspaceException(filePath);
        }
        if(await _fileSystemService.FileExistsAsync(filePath))
        {
            await _fileSystemService.WriteFileAsync(filePath, content, cancellationToken);
        }
        else
        {
            throw new FileNotFoundInWorkspaceException(filePath);
        }
    }

    public async Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if(!IsPathWithinWorkspace(filePath))
        {
            throw new PathOutsideWorkspaceException(filePath);
        }

        if (await _fileSystemService.DirectoryExistsAsync(filePath))
        {
            await _fileSystemService.DeleteDirectoryAsync(filePath, cancellationToken);
        }
        else if(await _fileSystemService.FileExistsAsync(filePath))
        {
            await _fileSystemService.DeleteFileAsync(filePath, cancellationToken);
        }
        else
        {
            throw new FileNotFoundInWorkspaceException(filePath);
        }
    }

    public async Task<WorkspaceItem> CreateFileAsync(string parentDirectoryPath, string fileName, CancellationToken cancellationToken = default)
    {
        if(!IsPathWithinWorkspace(parentDirectoryPath))
        {
            throw new PathOutsideWorkspaceException(parentDirectoryPath);
        }
        WorkspaceFileNameValidator.EnsureValid(fileName);

        var fullPath = Path.Combine(parentDirectoryPath, fileName);
        if (await _fileSystemService.FileExistsAsync(fullPath) || await _fileSystemService.DirectoryExistsAsync(fullPath))
        {
            throw new NodeAlreadyExistsException(fullPath);
        }

        await _fileSystemService.WriteFileAsync(fullPath, string.Empty, cancellationToken);

        return new WorkspaceItem(
                    Name: fileName, 
                    FullPath: fullPath, 
                    IsDirectory: false, 
                    Children: Array.Empty<WorkspaceItem>());
    }

    public async Task<WorkspaceItem> CreateFolderAsync(string parentDirectoryPath, string folderName, CancellationToken cancellationToken = default)
    {
        if(!IsPathWithinWorkspace(parentDirectoryPath))
        {
            throw new PathOutsideWorkspaceException(parentDirectoryPath);
        }
        WorkspaceFileNameValidator.EnsureValid(folderName);

        var fullPath = Path.Combine(parentDirectoryPath, folderName);
        if (await _fileSystemService.FileExistsAsync(fullPath) || await _fileSystemService.DirectoryExistsAsync(fullPath))
        {
            throw new NodeAlreadyExistsException(fullPath);
        }

        await _fileSystemService.CreateDirectoryAsync(fullPath);

        return new WorkspaceItem(
                    Name: folderName, 
                    FullPath: fullPath, 
                    IsDirectory: true, 
                    Children: Array.Empty<WorkspaceItem>());
    }

    public Task<WorkspaceItem> MoveAsync(string sourcePath, string destinationDirectoryPath, CancellationToken cancellationToken = default)
    {
        var destinationFullPath = Path.Combine(destinationDirectoryPath, Path.GetFileName(sourcePath));
        return MoveInternalAsync(sourcePath, destinationFullPath, cancellationToken);
    }

    public Task<WorkspaceItem> RenameAsync(string sourcePath, string newName, CancellationToken cancellationToken = default)
    {
        WorkspaceFileNameValidator.EnsureValid(newName);

        var parentDirectory = Path.GetDirectoryName(sourcePath)
            ?? throw new UndeterminedParentDirectoryException(sourcePath);
        var destinationFullPath = Path.Combine(parentDirectory, newName);

        return MoveInternalAsync(sourcePath, destinationFullPath, cancellationToken);
    }

    // ============== PRIVATE ==============
    private static bool IsSameOrDescendant(string sourceDirectoryPath, string destinationFullPath)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var normalizedSource = Path.TrimEndingDirectorySeparator(sourceDirectoryPath) + Path.DirectorySeparatorChar;
        var normalizedDestination = Path.TrimEndingDirectorySeparator(destinationFullPath) + Path.DirectorySeparatorChar;
        return normalizedDestination.StartsWith(normalizedSource, comparison);
    }
    private async Task<WorkspaceItem> MoveInternalAsync(string sourcePath, string destinationFullPath, CancellationToken cancellationToken)
    {
        if(!IsPathWithinWorkspace(sourcePath))
        {
            throw new PathOutsideWorkspaceException(sourcePath);
        }
        if(!IsPathWithinWorkspace(destinationFullPath))
        {
            throw new PathOutsideWorkspaceException(destinationFullPath);
        }

        var isDirectory = await _fileSystemService.DirectoryExistsAsync(sourcePath);

        if (isDirectory && IsSameOrDescendant(sourcePath, destinationFullPath))
        {
            throw new InvalidMoveException(sourcePath, destinationFullPath);
        }

        if (await _fileSystemService.FileExistsAsync(destinationFullPath) || await _fileSystemService.DirectoryExistsAsync(destinationFullPath))
        {
            throw new NodeAlreadyExistsException(destinationFullPath);
        }

        if (isDirectory)
        {
            await _fileSystemService.MoveDirectoryAsync(sourcePath, destinationFullPath, cancellationToken);
            // Rebuild the moved subtree so children carry their new paths too.
            var children = BuildTree(destinationFullPath, cancellationToken);
            return new WorkspaceItem(
                        Name: Path.GetFileName(destinationFullPath), 
                        FullPath: destinationFullPath, 
                        IsDirectory: true, 
                        Children: children);
        }

        await _fileSystemService.MoveFileAsync(sourcePath, destinationFullPath, cancellationToken);
        return new WorkspaceItem(
                    Name: Path.GetFileName(destinationFullPath), 
                    FullPath: destinationFullPath, 
                    IsDirectory: false, 
                    Children: Array.Empty<WorkspaceItem>());
    }
    private static List<WorkspaceItem> BuildTree(string directoryPath, CancellationToken cancellationToken = default)
    {
        var items = new List<WorkspaceItem>();

        IEnumerable<string> entries;
        try
        {
            entries = Directory.EnumerateFileSystemEntries(directoryPath);
        }
        catch(UnauthorizedAccessException)
        {
            return items;
        }

        foreach(var entryPath in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = Path.GetFileName(entryPath);
            var isDirectory = Directory.Exists(entryPath);

            if(isDirectory)
            {
                if(IgnoredDirectoryNames.Contains(name))
                {
                    continue;
                }

                var children = BuildTree(entryPath, cancellationToken);
                items.Add(new WorkspaceItem(name, entryPath, true, children));
            }
            else if (IsAllowedFile(entryPath))
            {
                items.Add(new WorkspaceItem(name, entryPath, false, []));
                
            }
        }

        return items.OrderByDescending(i => i.IsDirectory)
                        .ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
                        .ToList();
    }
    private static bool IsAllowedFile(string entryPath)
    {
        var extension = Path.GetExtension(entryPath);
        return WorkspaceFileTypeClassifier.IsAllowedExtension(extension);
    }

}