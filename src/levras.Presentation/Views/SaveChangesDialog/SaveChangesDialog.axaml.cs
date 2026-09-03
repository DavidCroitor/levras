using Avalonia.Controls;
using levras.Presentation.Services;

namespace levras.Presentation.Views;

public partial class SaveChangesDialog : Window
{
    public SaveChangesDialog()
    {
        InitializeComponent();
    }

    public SaveChangesDialog(string fileName): this()
    {
        MessageText.Text = $"\"{fileName}\" has unsaved changes. Do you want to save them?";
    }

    private void OnSaveClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(SaveChangesChoice.Save);
    private void OnDiscardClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(SaveChangesChoice.Discard);
    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(SaveChangesChoice.Cancel);

    
}