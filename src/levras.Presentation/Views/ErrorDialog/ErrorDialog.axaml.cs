using Avalonia.Controls;

namespace levras.Presentation.Views;

public partial class ErrorDialog : Window
{
    public ErrorDialog()
    {
        InitializeComponent();
        CanResize = false;
    }
    public ErrorDialog(string message): this ()
    {
        MessageText.Text = message;
    }

    private void OnOkClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(true);

}