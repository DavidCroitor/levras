
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Views;

public partial class WorkspaceExplorerView : UserControl
{
    private WorkspaceExplorerViewModel ViewModel => (WorkspaceExplorerViewModel)DataContext!;

    public WorkspaceExplorerView()
    {
        InitializeComponent();
    }

    private void OnNodeDoubleTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as Control)?.DataContext is WorkspaceItemViewModel { IsDirectory: false } node)
            ViewModel.BeginRenameCommand.Execute(node);
    }

    private void OnRenameTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if ((sender as TextBox)?.DataContext is not WorkspaceItemViewModel node) return;
        if (e.Key == Key.Enter) ViewModel.CommitRenameCommand.Execute(node);
        else if (e.Key == Key.Escape) ViewModel.CancelRenameCommand.Execute(node);
    }

    private void OnRenameTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if ((sender as TextBox)?.DataContext is WorkspaceItemViewModel { IsEditing: true } node)
            ViewModel.CommitRenameCommand.Execute(node);
    }

}