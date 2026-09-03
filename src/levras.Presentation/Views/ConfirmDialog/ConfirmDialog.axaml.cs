using Avalonia.Controls;

namespace levras.Presentation.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }
    public ConfirmDialog(string title, string message): this ()
    {
        Title = title;
        MessageText.Text = message;
    }

    private void OnYesClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(true);
    private void OnNoClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(false);

}