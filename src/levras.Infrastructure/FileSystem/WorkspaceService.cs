using System.Data.Common;
using System.Runtime.CompilerServices;
using levras.Core.Abstractions;
using levras.Core.Exceptions;
using levras.Core.Models;

namespace levras.Infrastructure.FileSystem;

public sealed class WorkspaceService : IWorkspaceService
{
    public string? CurrentWorkspacePath {get; private set;}
    private static readonly HashSet<string> IgnoredDirectoryNames = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
      ".md", ".markdown",
      ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp", ".bmp"  
    };

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
        return AllowedFileExtensions.Contains(extension);
    }
}