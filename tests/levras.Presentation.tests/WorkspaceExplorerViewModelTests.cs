using levras.Core.Abstractions;
using levras.Core.Models;
using NSubstitute;
using levras.Presentation.Services;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Tests;

public class WorkspaceExplorerViewModelTests
{
    private readonly IWorkspaceService _workspaceService = Substitute.For<IWorkspaceService>();
    private readonly IFileSystemService _fileSystemService = Substitute.For<IFileSystemService>();
    private readonly IDialogService _dialogService = Substitute.For<IDialogService>();
    private readonly WorkspaceExplorerViewModel _sut;

    public WorkspaceExplorerViewModelTests()
    {
        _workspaceService.IsPathWithinWorkspace(Arg.Any<string>()).Returns(true);
        _sut = new WorkspaceExplorerViewModel(_workspaceService, _dialogService, _fileSystemService);
    }

    private async Task<WorkspaceItemViewModel> LoadSingleFileWorkspaceAsync(string fileName = "notes.md")
    {
        var tree = new List<WorkspaceItem>
        {
            new(fileName, $"/workspace/{fileName}", IsDirectory: false, Children: [])
        };
        _workspaceService.GetWorkspaceTreeAsync(Arg.Any<CancellationToken>()).Returns(tree);

        await _sut.LoadWorkspaceAsync("/workspace");

        return _sut.RootItems.Single();
    }

    [Fact]
    public async Task DeleteFileCommand_WhenUserConfirms_DeletesFileAndRemovesFromTree()
    {
        var item = await LoadSingleFileWorkspaceAsync();
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await _sut.DeleteFileCommand.ExecuteAsync(item);

        await _fileSystemService.Received(1).DeleteFileAsync(item.FullPath, Arg.Any<CancellationToken>());
        Assert.DoesNotContain(item, _sut.RootItems);
    }

    [Fact]
    public async Task DeleteFileCommand_WhenUserCancels_DoesNotDeleteOrModifyTree()
    {
        var item = await LoadSingleFileWorkspaceAsync();
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        await _sut.DeleteFileCommand.ExecuteAsync(item);

        await _fileSystemService.DidNotReceive().DeleteFileAsync(
            Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.Contains(item, _sut.RootItems);
    }

    [Fact]
    public async Task DeleteFileCommand_RaisesFileDeletedEvent_WithCorrectPath()
    {
        var item = await LoadSingleFileWorkspaceAsync();
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        string? raisedPath = null;
        _sut.FileDeleted += (_, path) => raisedPath = path;

        await _sut.DeleteFileCommand.ExecuteAsync(item);

        Assert.Equal(item.FullPath, raisedPath);
    }

    [Fact]
    public async Task DeleteFileCommand_WhenIoFails_ShowsErrorAndLeavesTreeUnchanged()
    {
        var item = await LoadSingleFileWorkspaceAsync();
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _fileSystemService.DeleteFileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new Core.Exceptions.AccessDeniedException(item.FullPath));

        await _sut.DeleteFileCommand.ExecuteAsync(item);

        await _dialogService.Received(1).ShowErrorAsync(Arg.Any<string>());
        Assert.Contains(item, _sut.RootItems); // tree unchanged since deletion failed
    }
    [Fact]
    public async Task DeleteFileCommand_WithNestedFile_RemovesFromParentsChildrenCollection()
    {
        var tree = new List<WorkspaceItem>
        {
            new("SubFolder", "/workspace/SubFolder", IsDirectory: true, Children:
            [
                new("nested.md", "/workspace/SubFolder/nested.md", IsDirectory: false, Children: [])
            ])
        };
        _workspaceService.GetWorkspaceTreeAsync(Arg.Any<CancellationToken>()).Returns(tree);
        await _sut.LoadWorkspaceAsync("/workspace");

        var folder = _sut.RootItems.Single();
        var nestedFile = folder.Children.Single();
        _dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await _sut.DeleteFileCommand.ExecuteAsync(nestedFile);

        Assert.Empty(folder.Children);
    }
}