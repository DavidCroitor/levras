using levras.Core.Abstractions;
using levras.Presentation.Services;
using levras.Presentation.ViewModels;
using NSubstitute;

namespace levras.Presentation.Tests;

public class MainViewModelTests
{
    private readonly IWorkspaceService _workspaceService = Substitute.For<IWorkspaceService>();
    private readonly IDialogService _dialogService = Substitute.For<IDialogService>();
    private readonly MainViewModel _sut;

    public MainViewModelTests()
    {
        _workspaceService.CurrentWorkspacePath.Returns("/workspace");
        _workspaceService.GetWorkspaceTreeAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<levras.Core.Domain.WorkspaceItem>());

        var explorer = new WorkspaceExplorerViewModel(_workspaceService, _dialogService);
        _sut = new MainViewModel(
            explorer,
            _dialogService,
            Substitute.For<IFolderPickerService>(),
            Substitute.For<ITabFactory>());
    }

    [Fact]
    public async Task CloseWorkspaceCommand_ClosesAllTabsAndHidesWorkspace()
    {
        await _sut.Explorer.LoadWorkspaceAsync("/workspace");
        _sut.OpenTabs.Add(new ImageViewerTabViewModel("/workspace/image.png", _workspaceService));
        var editor = await CreateDirtyEditorAsync("/workspace/notes.md");
        _sut.OpenTabs.Add(editor);
        _dialogService.ConfirmSaveChangesAsync(Arg.Any<string>())
            .Returns(SaveChangesChoice.Discard);

        await _sut.CloseWorkspaceCommand.ExecuteAsync(null);

        Assert.Empty(_sut.OpenTabs);
        Assert.False(_sut.Explorer.IsWorkspaceOpen);
        Assert.Empty(_sut.Explorer.RootItems);
    }

    [Fact]
    public async Task CloseWorkspaceCommand_WhenSavePromptIsCanceled_PreservesWorkspaceAndTabs()
    {
        await _sut.Explorer.LoadWorkspaceAsync("/workspace");
        var editor = await CreateDirtyEditorAsync("/workspace/notes.md");
        _sut.OpenTabs.Add(editor);
        _dialogService.ConfirmSaveChangesAsync(Arg.Any<string>())
            .Returns(SaveChangesChoice.Cancel);

        await _sut.CloseWorkspaceCommand.ExecuteAsync(null);

        Assert.Single(_sut.OpenTabs);
        Assert.Same(editor, _sut.OpenTabs[0]);
        Assert.True(_sut.Explorer.IsWorkspaceOpen);
    }

    private async Task<TextEditorTabViewModel> CreateDirtyEditorAsync(string path)
    {
        var editor = new TextEditorTabViewModel(path, _workspaceService, _dialogService);
        _workspaceService.ReadFileAsync(path, Arg.Any<CancellationToken>()).Returns("initial");
        await editor.LoadFileAsync();
        editor.Document.Text = "changed";
        return editor;
    }
}
