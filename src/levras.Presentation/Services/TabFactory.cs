
using System.Threading;
using System.Threading.Tasks;
using levras.Core.Abstractions;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Services;

public class TabFactory : ITabFactory
{
    private readonly IWorkspaceService _workspaceService;
    private readonly IDialogService _dialogService;

    public TabFactory(IWorkspaceService workspaceService, IDialogService dialogService)
    {
        _workspaceService = workspaceService;
        _dialogService = dialogService;
    }

    public async Task<TabViewModelBase> CreateTabAsync(WorkspaceItemViewModel node, CancellationToken cancellationToken = default)
    {
        if (node.IsImageFile)
        {
            var imageTab = new ImageViewerTabViewModel(node.FullPath, _workspaceService);
            await imageTab.LoadAsync(cancellationToken);
            return imageTab;
        }

        var textTab = new TextEditorTabViewModel(node.FullPath, _workspaceService, _dialogService);
        await textTab.LoadFileAsync(cancellationToken);
        return textTab;
    }
}