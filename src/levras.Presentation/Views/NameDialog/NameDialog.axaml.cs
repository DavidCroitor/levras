using Avalonia.Controls;

namespace levras.Presentation.Views;

public partial class NameDialog : Window
{
    public NameDialog()
    {
        InitializeComponent();
    }

    public NameDialog(string title, string message, string defaultValue = "New Folder") : this()
    {
        Title = title;
        MessageText.Text = message;
        NameTextBox.Text = defaultValue;
        NameTextBox.SelectAll();
    }

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(null);
    private void OnDoneClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var name = NameTextBox.Text?.Trim();
        if(!string.IsNullOrWhiteSpace(name))
        {
            Close(name);
        }
    }
    

}