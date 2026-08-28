using levras.Core.Domain;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Tests;

public class WorkspaceItemViewModelTests
{
    [Theory]
    [InlineData("notes.md", true)]
    [InlineData("notes.markdown", true)]
    [InlineData("NOTES.MD", true)] // case-insensitivity
    [InlineData("diagram.png", false)]
    [InlineData("photo.jpg", false)]
    [InlineData("readme.txt", false)]
    public void IsTextFile_ReflectsExtension(string fileName, bool expected)
    {
        var item = new WorkspaceItem(fileName, $"/workspace/{fileName}", IsDirectory: false, Children: []);
        var sut = new WorkspaceItemViewModel(item);

        Assert.Equal(expected, sut.IsTextFile);
    }

    [Fact]
    public void IsTextFile_ForDirectory_IsFalseEvenWithMdLikeName()
    {
        var item = new WorkspaceItem("notes.md", "/workspace/notes.md", IsDirectory: true, Children: []);
        var sut = new WorkspaceItemViewModel(item);

        Assert.False(sut.IsTextFile);
    }
}