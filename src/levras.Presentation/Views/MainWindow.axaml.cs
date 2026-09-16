using Avalonia.Controls;
using Avalonia.Input;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Views;

public partial class MainWindow : Window
{
    private bool _ctrlWPressed;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if(e.Key != Key.W || !e.KeyModifiers.HasFlag(KeyModifiers.Control)) return;

        e.Handled = true;

        if(_ctrlWPressed) return;

        _ctrlWPressed = true;

        if(DataContext is MainViewModel viewModel && viewModel.CloseTabCommand.CanExecute(viewModel.SelectedTab))
        {
            viewModel.CloseTabCommand.Execute(viewModel.SelectedTab);
        }
    }
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if(e.Key is Key.W or Key.LeftCtrl or Key.RightCtrl)
        {
            _ctrlWPressed = false;
        }
    }
}