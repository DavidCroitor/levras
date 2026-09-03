using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using levras.Core.Abstractions;

namespace levras.Presentation.ViewModels;

public partial class ImageViewerTabViewModel : TabViewModelBase
{
    private readonly IWorkspaceService _workspaceService;
    [ObservableProperty] private Bitmap? _image;

    public ImageViewerTabViewModel(string filePath, IWorkspaceService workspaceService)
        : base(filePath, Path.GetFileName(filePath))
    {
        _workspaceService = workspaceService;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var bytes = await _workspaceService.ReadFileBytesAsync(FilePath, cancellationToken);
        using var stream = new MemoryStream(bytes);
        Image = new Bitmap(stream);
    }
}