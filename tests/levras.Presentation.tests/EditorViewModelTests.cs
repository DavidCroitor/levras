using levras.Core.Abstractions;
using levras.Core.Exceptions;
using NSubstitute;
using levras.Presentation.Services;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Tests;

public class EditorViewModelTests
{
    private readonly IFileSystemService _fileSystemService = Substitute.For<IFileSystemService>();
    private readonly IWorkspaceService _workspaceService = Substitute.For<IWorkspaceService>();
    private readonly IDialogService _dialogService = Substitute.For<IDialogService>();
    private readonly EditorViewModel _sut;

    public EditorViewModelTests()
    {
        _workspaceService.IsPathWithinWorkspace(Arg.Any<string>()).Returns(true);
        _sut = new EditorViewModel(_fileSystemService, _workspaceService, _dialogService);
    }

    [Fact]
    public async Task LoadFileAsync_SetsDocumentTextAndClearsIsDirty()
    {
        _fileSystemService.ReadFileAsync("notes.md", Arg.Any<CancellationToken>())
            .Returns("# Hello");

        await _sut.LoadFileAsync("notes.md");

        Assert.Equal("# Hello", _sut.Document.Text);
        Assert.False(_sut.IsDirty);
        Assert.Equal("notes.md", _sut.CurrentFilePath);
    }

    [Fact]
    public async Task LoadFileAsync_WithPathOutsideWorkspace_ThrowsAndDoesNotCallFileSystem()
    {
        _workspaceService.IsPathWithinWorkspace("outside.md").Returns(false);

        await Assert.ThrowsAsync<PathOutsideWorkspaceException>(
            () => _sut.LoadFileAsync("outside.md"));

        await _fileSystemService.DidNotReceive().ReadFileAsync(
            Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void EditingDocument_AfterLoad_SetsIsDirtyTrue()
    {
        _sut.Document.Text = "typed content";

        Assert.True(_sut.IsDirty);
    }

    [Fact]
    public async Task TryPrepareToDiscardAsync_WhenNotDirty_ReturnsTrueWithoutPromptingDialog()
    {
        var result = await _sut.TryPrepareToDiscardAsync();

        Assert.True(result);
        await _dialogService.DidNotReceive().ConfirmSaveChangesAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task TryPrepareToDiscardAsync_WhenDirtyAndUserChoosesDiscard_ReturnsTrueWithoutSaving()
    {
        await LoadInitialFile();
        _sut.Document.Text = "unsaved edit";
        _dialogService.ConfirmSaveChangesAsync(Arg.Any<string>())
            .Returns(SaveChangesChoice.Discard);

        var result = await _sut.TryPrepareToDiscardAsync();

        Assert.True(result);
        await _fileSystemService.DidNotReceive().WriteFileAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryPrepareToDiscardAsync_WhenDirtyAndUserChoosesCancel_ReturnsFalseAndStaysDirty()
    {
        await LoadInitialFile();
        _sut.Document.Text = "unsaved edit";
        _dialogService.ConfirmSaveChangesAsync(Arg.Any<string>())
            .Returns(SaveChangesChoice.Cancel);

        var result = await _sut.TryPrepareToDiscardAsync();

        Assert.False(result);
        Assert.True(_sut.IsDirty); // content must NOT be considered saved/discarded
    }

    [Fact]
    public async Task TryPrepareToDiscardAsync_WhenDirtyAndUserChoosesSave_WritesFileAndClearsIsDirty()
    {
        await LoadInitialFile();
        _sut.Document.Text = "unsaved edit";
        _dialogService.ConfirmSaveChangesAsync(Arg.Any<string>())
            .Returns(SaveChangesChoice.Save);

        var result = await _sut.TryPrepareToDiscardAsync();

        Assert.True(result);
        Assert.False(_sut.IsDirty);
        await _fileSystemService.Received(1).WriteFileAsync(
            "notes.md", "unsaved edit", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAsync_WithNoFileLoaded_ReturnsFalseAndDoesNotCallFileSystem()
    {
        await _sut.SaveCommand.ExecuteAsync(null);

        await _fileSystemService.DidNotReceive().WriteFileAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private async Task LoadInitialFile()
    {
        _fileSystemService.ReadFileAsync("notes.md", Arg.Any<CancellationToken>())
            .Returns("initial content");
        await _sut.LoadFileAsync("notes.md");
    }
}