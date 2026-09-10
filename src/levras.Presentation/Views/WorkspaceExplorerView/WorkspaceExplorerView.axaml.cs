using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Views;

public partial class WorkspaceExplorerView : UserControl
{
    // Custom in-process format for carrying the dragged view model by reference.
    // Using our own format (rather than e.g. text) means we only ever react
    // to drags that originated from this tree, not from the OS shell etc.
    private static readonly DataFormat<WorkspaceItemViewModel> NodeFormat =
        DataFormat.CreateInProcessFormat<WorkspaceItemViewModel>("levras.workspace-node");

    private WorkspaceExplorerViewModel ViewModel => (WorkspaceExplorerViewModel)DataContext!;

    private Point _dragStartPoint;
    private WorkspaceItemViewModel? _dragCandidate;
    private PointerPressedEventArgs? _dragPressArgs;
    private bool _dragInProgress;

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

    // ==================== DRAG SOURCE ====================

    private void OnRowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control)
        {
            Debug.WriteLine("[DragDrop] PointerPressed ignored: sender is not a Control.");
            return;
        }
        if (!e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
        {
            Debug.WriteLine("[DragDrop] PointerPressed ignored: left button is not pressed.");
            return;
        }

        // Don't start a drag out from under an active rename textbox.
        if (control.DataContext is not WorkspaceItemViewModel { IsEditing: false } node)
        {
            Debug.WriteLine("[DragDrop] PointerPressed ignored: row has no editable node.");
            return;
        }

        _dragStartPoint = e.GetPosition(null);
        _dragCandidate = node;
        _dragPressArgs = e;
        _dragInProgress = false;
        Debug.WriteLine($"[DragDrop] Candidate selected: {node.Name}.");
    }

    private async void OnRowPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragCandidate is null || _dragInProgress) return;
        if (sender is not Control control) return;
        if (!e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var current = e.GetPosition(null);
        var delta = current - _dragStartPoint;

        // Require a small movement threshold so ordinary clicks (selection,
        // expand/collapse, rename) don't get hijacked as drag gestures.
        if (Math.Abs(delta.X) < 4 && Math.Abs(delta.Y) < 4) return;

        var node = _dragCandidate;
        var pressArgs = _dragPressArgs;
        if (pressArgs is null)
        {
            return;
        }

        _dragInProgress = true;
        node.IsBeingDragged = true;

        var data = new DataTransfer();
        data.Add(DataTransferItem.Create(NodeFormat, node));


        try
        {
            var result = await DragDrop.DoDragDropAsync(pressArgs, data, DragDropEffects.Move);
        }
        finally
        {
            node.IsBeingDragged = false;
            _dragCandidate = null;
            _dragPressArgs = null;
            _dragInProgress = false;
        }
    }

    private void OnRowPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragCandidate is not null)
        _dragCandidate = null;
        _dragPressArgs = null;
        _dragInProgress = false;
    }

    // ==================== DROP TARGET (tree row = a folder or file) ====================

    private void OnRowDragEnter(object? sender, DragEventArgs e) => UpdateRowDropState(sender, e);

    private void OnRowDragOver(object? sender, DragEventArgs e) => UpdateRowDropState(sender, e);

    private void OnRowDragLeave(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is WorkspaceItemViewModel node)
        {
            node.IsDropTarget = false;
        }
    }

    private void OnRowDrop(object? sender, DragEventArgs e)
    {
        if ((sender as Control)?.DataContext is not WorkspaceItemViewModel targetNode)
        {
            return;
        }
        targetNode.IsDropTarget = false;

        var hasSource = TryGetDraggedNode(e, out var sourceNode);
        var canMove = hasSource && ViewModel.CanMoveNode(sourceNode, targetNode);
        if (canMove)
        {
            ViewModel.MoveNodeCommand.Execute((sourceNode, (WorkspaceItemViewModel?)targetNode));
        }

        // Stop the event bubbling up to the TreeView-level (root) drop handler.
        e.Handled = true;
    }

    private void UpdateRowDropState(object? sender, DragEventArgs e)
    {
        if ((sender as Control)?.DataContext is not WorkspaceItemViewModel targetNode)
        {
            Debug.WriteLine("[DragDrop] Row DragOver ignored: target has no node data.");
            e.DragEffects = DragDropEffects.None;
            return;
        }

        var hasSource = TryGetDraggedNode(e, out var sourceNode);
        var valid = hasSource && ViewModel.CanMoveNode(sourceNode, targetNode);

        targetNode.IsDropTarget = valid;
        e.DragEffects = valid ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    // ==================== DROP TARGET (empty tree area = workspace root) ====================

    private void OnTreeDragOver(object? sender, DragEventArgs e)
    {
        var hasSource = TryGetDraggedNode(e, out var sourceNode);
        var valid = hasSource && ViewModel.CanMoveNode(sourceNode, null);
        e.DragEffects = valid
            ? DragDropEffects.Move
            : DragDropEffects.None;
    }

    private void OnTreeDrop(object? sender, DragEventArgs e)
    {
        var hasSource = TryGetDraggedNode(e, out var sourceNode);
        var canMove = hasSource && ViewModel.CanMoveNode(sourceNode, null);
        if (canMove)
        {
            ViewModel.MoveNodeCommand.Execute((sourceNode, (WorkspaceItemViewModel?)null));
        }
    }

    private static bool TryGetDraggedNode(DragEventArgs e, out WorkspaceItemViewModel node)
    {
        var found = e.DataTransfer.TryGetValue(NodeFormat);
        if (found is not null)
        {
            node = found;
            return true;
        }

        node = null!;
        return false;
    }
}