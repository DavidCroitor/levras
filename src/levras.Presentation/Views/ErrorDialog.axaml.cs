using Avalonia.Controls;

namespace levras.Presentation.Views;

public partial class ErrorDialog : Window
{
    public ErrorDialog()
    {
        InitializeComponent();
    }
    public ErrorDialog(string title, string message): this ()
    {
        Title = title;
        MessageText.Text = message;
    }

    private void OnOkClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(true);

}