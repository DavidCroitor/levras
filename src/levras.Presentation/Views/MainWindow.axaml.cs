using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Views;

public partial class MainWindow : Window
{
    private bool _closeTabKeyHeld;

    public MainWindow()
    {
        InitializeComponent();

        AddHandler(KeyDownEvent, OnCloseTabKeyDown, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, OnCloseTabKeyUp, RoutingStrategies.Tunnel);
    }

    private void OnCloseTabKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.W || e.KeyModifiers != KeyModifiers.Control)
            return;

        // Swallow every auto-repeat KeyDown for the same held press.
        e.Handled = true;

        if (_closeTabKeyHeld)
            return;

        _closeTabKeyHeld = true;

        if (DataContext is MainViewModel vm &&
            vm.CloseTabCommand.CanExecute(vm.SelectedTab))
        {
            vm.CloseTabCommand.Execute(vm.SelectedTab);
        }
    }

    private void OnCloseTabKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.W || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _closeTabKeyHeld = false;
        }
    }
}