using System;
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
        var owner = GetMainWindow()
        ?? throw new InvalidOperationException("No owner window available for dialog.");


        var dialog = new SaveChangesDialog(fileName);
        var result = await dialog.ShowDialog<SaveChangesChoice>(owner);
        return result;
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
    public async Task<string?> PromptNameAsync(string title, string message, string defaultValue = "New Folder")
    {
        var owner = GetMainWindow();
        var dialog = new NameDialog(title, message, defaultValue);
        if(owner is null)
        {
            return await dialog.ShowDialog<string?>(new Window());
        }
        return await dialog.ShowDialog<string?>(owner);
    }

    private Window? GetMainWindow()
    {
        return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null;
    }


}