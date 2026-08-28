using levras.Core.Exceptions;
using levras.Infrastructure.FileSystem;

namespace levras.Infrastructure.Tests;

public class FileSystemServiceTests : IDisposable
{
    private readonly string _testRoot;
    private readonly FileSystemService _sut;

    public FileSystemServiceTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "MarkdownEditorTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testRoot);
        _sut = new FileSystemService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DeleteFileAsync_WithExistingFile_RemovesItFromDisk()
    {
        var filePath = Path.Combine(_testRoot, "notes.md");
        File.WriteAllText(filePath, "content");

        await _sut.DeleteFileAsync(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task DeleteFileAsync_WithNonExistentFile_ThrowsFileNotFoundInWorkspaceException()
    {
        var filePath = Path.Combine(_testRoot, "missing.md");

        await Assert.ThrowsAsync<FileNotFoundInWorkspaceException>(
            () => _sut.DeleteFileAsync(filePath));
    }

    [Fact]
    public async Task DeleteFileAsync_DoesNotDeleteOtherFilesInSameFolder()
    {
        var targetPath = Path.Combine(_testRoot, "delete-me.md");
        var otherPath = Path.Combine(_testRoot, "keep-me.md");
        File.WriteAllText(targetPath, "content");
        File.WriteAllText(otherPath, "content");

        await _sut.DeleteFileAsync(targetPath);

        Assert.True(File.Exists(otherPath));
    }
}