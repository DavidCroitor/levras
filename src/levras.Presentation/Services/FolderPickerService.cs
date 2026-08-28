using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace levras.Presentation.Services;

public sealed class FolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync()
    {
        var mainWindow = GetMainWindow();
        if(mainWindow?.StorageProvider is null)
        {
            return null;
        }

        var result = await mainWindow.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Open Workspace Folder",
                AllowMultiple = false
            }
        );
        var folder = result.FirstOrDefault();
        return folder?.TryGetLocalPath();
    }

    private static Window? GetMainWindow()
    {
        return Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }
}