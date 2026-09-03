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
    public async Task<bool> ConfirmAsync(string title, string message)
    {
        var owner = GetMainWindow();
        var dialog = new ConfirmDialog(title, message);

        if(owner is null)
        {
            return await dialog.ShowDialog<bool>(new Window());
        }
        return await dialog.ShowDialog<bool>(owner);

    }
    public async Task ShowErrorAsync(string message)
    {
        var owner = GetMainWindow();
        var dialog = new ErrorDialog("Error", message);

        if (owner is null)
        {
            await dialog.ShowDialog<bool>(new Window());
            return;
        }

        await dialog.ShowDialog<bool>(owner);
    }
    public Task<string> PromptForNameAsync(string message)
    {
        throw new System.NotImplementedException();
    }

    private Window? GetMainWindow()
    {
        return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null;
    }
}