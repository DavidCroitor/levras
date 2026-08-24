using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using levras.Core.Abstractions;

namespace levras.Presentation.ViewModels;

public partial class WorkspaceExplorerViewModel : ViewModelBase
{
    private readonly IWorkspaceService _workspaceService;
    private WorkspaceItemViewModel? _currentlySelected;
    private bool _isSyncingSelection;

    public ObservableCollection<WorkspaceItemViewModel> RootItems { get; } = new();

    public event EventHandler<string>? FileSelected;

    public WorkspaceExplorerViewModel(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public async Task LoadWorkspaceAsync(string folderPath)
    {
        _workspaceService.OpenWorkspace(folderPath);
        var tree = await _workspaceService.GetWorkspaceTreeAsync();

        RootItems.Clear();
        _currentlySelected = null;
        foreach (var item in tree)
        {
            var vm = new WorkspaceItemViewModel(item);
            WireSelection(vm);
            RootItems.Add(vm);
        }
    }

    private void WireSelection(WorkspaceItemViewModel node)
    {
        node.SelectionRequested = OnNodeSelectionRequested;
        foreach (var child in node.Children)
        {
            WireSelection(child);
        }
    }

    private void OnNodeSelectionRequested(WorkspaceItemViewModel node)
    {
        if (_isSyncingSelection || node.IsDirectory || node == _currentlySelected)
        {
            return;
        }

        SetSyncedSelection(false); // deselect old node's visual state
        _currentlySelected = node;

        FileSelected?.Invoke(this, node.FullPath);
    }

    public void RevertSelectionTo(string? filePath)
    {
        SetSyncedSelection(false);
        _currentlySelected = filePath is null ? null : FindItemByPath(RootItems, filePath);
        SetSyncedSelection(true);
    }

    private void SetSyncedSelection(bool selected)
    {
        if (_currentlySelected is null) return;
        _isSyncingSelection = true;
        try { _currentlySelected.IsSelected = selected; }
        finally { _isSyncingSelection = false; }
    }

    private static WorkspaceItemViewModel? FindItemByPath(
        IEnumerable<WorkspaceItemViewModel> items, string filePath)
    {
        foreach (var item in items)
        {
            if (!item.IsDirectory && item.FullPath == filePath) return item;
            if (item.IsDirectory)
            {
                var found = FindItemByPath(item.Children, filePath);
                if (found is not null) return found;
            }
        }
        return null;
    }
}