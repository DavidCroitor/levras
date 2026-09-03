using Avalonia.Controls;
using AvaloniaEdit.Document;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Views;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext!;
    public MainWindow()
    {
        InitializeComponent();
    }
}