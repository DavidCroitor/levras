using levras.Core.Exceptions;
using levras.Infrastructure.FileSystem;

namespace levras.Infrastructure.Tests;

public class WorkspaceServiceTests : IDisposable
{
    private readonly string _testRoot;
    private readonly WorkspaceService _sut;

    public WorkspaceServiceTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "MarkdownEditorTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testRoot);
        _sut = new WorkspaceService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, recursive: true);
        }
    }

    [Fact]
    public void OpenWorkspace_WithValidFolder_SetsCurrentWorkspacePath()
    {
        _sut.OpenWorkspace(_testRoot);

        Assert.Equal(Path.GetFullPath(_testRoot), _sut.CurrentWorkspacePath);
    }

    [Fact]
    public void OpenWorkspace_WithNonExistentFolder_ThrowsWorkspaceNotFoundException()
    {
        var missingPath = Path.Combine(_testRoot, "does-not-exist");

        Assert.Throws<WorkspaceNotFoundException>(() => _sut.OpenWorkspace(missingPath));
    }

    [Fact]
    public void IsPathWithinWorkspace_WithNoWorkspaceOpen_ReturnsFalse()
    {
        var result = _sut.IsPathWithinWorkspace(Path.Combine(_testRoot, "file.md"));

        Assert.False(result);
    }

    [Fact]
    public void IsPathWithinWorkspace_WithPathInsideWorkspace_ReturnsTrue()
    {
        _sut.OpenWorkspace(_testRoot);
        var innerPath = Path.Combine(_testRoot, "notes", "file.md");

        Assert.True(_sut.IsPathWithinWorkspace(innerPath));
    }

    [Fact]
    public void IsPathWithinWorkspace_WithSiblingFolderThatSharesPrefix_ReturnsFalse()
    {
        // Regression test for the classic StartsWith bug: a workspace at
        // ".../MyWorkspace" must NOT consider ".../MyWorkspace-Evil" to be inside it.
        _sut.OpenWorkspace(_testRoot);
        var siblingPath = _testRoot + "-Evil" + Path.DirectorySeparatorChar + "file.md";

        Assert.False(_sut.IsPathWithinWorkspace(siblingPath));
    }

    [Fact]
    public void IsPathWithinWorkspace_WithPathTraversalOutOfWorkspace_ReturnsFalse()
    {
        _sut.OpenWorkspace(_testRoot);
        var traversalPath = Path.Combine(_testRoot, "..", "outside.md");

        Assert.False(_sut.IsPathWithinWorkspace(traversalPath));
    }

    [Fact]
    public async Task GetWorkspaceTreeAsync_FiltersOutDisallowedFileExtensions()
    {
        File.WriteAllText(Path.Combine(_testRoot, "notes.md"), "content");
        File.WriteAllText(Path.Combine(_testRoot, "readme.txt"), "content");
        File.WriteAllText(Path.Combine(_testRoot, "diagram.png"), "content");
        _sut.OpenWorkspace(_testRoot);

        var tree = await _sut.GetWorkspaceTreeAsync();

        Assert.Equal(2, tree.Count);
        Assert.Contains(tree, item => item.Name == "notes.md");
        Assert.Contains(tree, item => item.Name == "diagram.png");
        Assert.DoesNotContain(tree, item => item.Name == "readme.txt");
    }

    [Fact]
    public async Task GetWorkspaceTreeAsync_IncludesSubfoldersRegardlessOfExtensionFilter()
    {
        Directory.CreateDirectory(Path.Combine(_testRoot, "SubFolder"));
        _sut.OpenWorkspace(_testRoot);

        var tree = await _sut.GetWorkspaceTreeAsync();

        var folder = Assert.Single(tree);
        Assert.True(folder.IsDirectory);
        Assert.Equal("SubFolder", folder.Name);
    }

    [Fact]
    public async Task GetWorkspaceTreeAsync_BuildsNestedChildrenRecursively()
    {
        var subFolder = Path.Combine(_testRoot, "SubFolder");
        Directory.CreateDirectory(subFolder);
        File.WriteAllText(Path.Combine(subFolder, "nested.md"), "content");
        _sut.OpenWorkspace(_testRoot);

        var tree = await _sut.GetWorkspaceTreeAsync();

        var folder = Assert.Single(tree);
        var nestedFile = Assert.Single(folder.Children);
        Assert.Equal("nested.md", nestedFile.Name);
        Assert.False(nestedFile.IsDirectory);
    }

    [Fact]
    public async Task GetWorkspaceTreeAsync_SortsFoldersBeforeFiles_ThenAlphabetically()
    {
        File.WriteAllText(Path.Combine(_testRoot, "zebra.md"), "content");
        File.WriteAllText(Path.Combine(_testRoot, "apple.md"), "content");
        Directory.CreateDirectory(Path.Combine(_testRoot, "Zulu"));
        Directory.CreateDirectory(Path.Combine(_testRoot, "Alpha"));
        _sut.OpenWorkspace(_testRoot);

        var tree = await _sut.GetWorkspaceTreeAsync();

        Assert.Equal(
            new[] { "Alpha", "Zulu", "apple.md", "zebra.md" },
            tree.Select(item => item.Name));
    }

    [Fact]
    public async Task GetWorkspaceTreeAsync_WithoutOpenWorkspace_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GetWorkspaceTreeAsync());
    }
}