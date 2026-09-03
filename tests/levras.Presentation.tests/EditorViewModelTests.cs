using levras.Core.Abstractions;
using levras.Core.Exceptions;
using NSubstitute;
using levras.Presentation.Services;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Tests;

public class EditorViewModelTests
{
    private readonly IWorkspaceService _workspaceService = Substitute.For<IWorkspaceService>();
    private readonly IDialogService _dialogService = Substitute.For<IDialogService>();
    private readonly TextEditorTabViewModel _sut;

    public EditorViewModelTests()
    {
        _sut = new TextEditorTabViewModel("notes.md", _workspaceService, _dialogService);
    }

    [Fact]
    public async Task LoadFileAsync_SetsDocumentTextAndClearsIsDirty()
    {
        _workspaceService.ReadFileAsync("notes.md", Arg.Any<CancellationToken>())
            .Returns("# Hello");

        await _sut.LoadFileAsync();

        Assert.Equal("# Hello", _sut.Document.Text);
        Assert.False(_sut.IsDirty);
        Assert.Equal("notes.md", _sut.FilePath);
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
        await _workspaceService.DidNotReceive().WriteFileAsync(
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
        await _workspaceService.Received(1).WriteFileAsync(
            "notes.md", "unsaved edit", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAsync_WritesFileAndClearsIsDirty()
    {
        await LoadInitialFile();
        _sut.Document.Text = "new content";

        await _sut.SaveCommand.ExecuteAsync(null);

        Assert.False(_sut.IsDirty);
        await _workspaceService.Received(1).WriteFileAsync(
            "notes.md", "new content", Arg.Any<CancellationToken>());
    }

    private async Task LoadInitialFile()
    {
        _workspaceService.ReadFileAsync("notes.md", Arg.Any<CancellationToken>())
            .Returns("initial content");
        await _sut.LoadFileAsync();
    }
}