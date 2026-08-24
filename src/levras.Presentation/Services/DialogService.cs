using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using levras.Presentation.Views;

namespace levras.Presentation.Services;

public sealed class DialogService : IDialogService
{
    public async Task<SaveChangesChoice> ConfirmSaveChangesAsync(string fileName)
    {
        var owner = GetMainWindow();
        var dialog = new SaveChangesDialog(fileName);

        if(owner is null)
        {
            return await dialog.ShowDialog<SaveChangesChoice>(new Window());
        }

        return await dialog.ShowDialog<SaveChangesChoice>(owner);
    }

    private Window? GetMainWindow()
    {
        return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null;
    }
}