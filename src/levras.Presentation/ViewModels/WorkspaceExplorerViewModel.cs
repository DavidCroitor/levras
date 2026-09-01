using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using levras.Core.Abstractions;
using levras.Core.Exceptions;
using levras.Presentation.Services;

namespace levras.Presentation.ViewModels;

public partial class WorkspaceExplorerViewModel : ViewModelBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly IDialogService _dialogService;
    private readonly IFileSystemService _fileSystemService;
    private WorkspaceItemViewModel? _currentlySelected;
    private bool _isSyncingSelection;

    public ObservableCollection<WorkspaceItemViewModel> RootItems { get; } = new();

    public event EventHandler<WorkspaceItemViewModel>? FileSelected;
    public event EventHandler<string>? FileDeleted;

    public WorkspaceExplorerViewModel(
        IWorkspaceService workspaceService,
        IDialogService dialogService,
        IFileSystemService fileSystemService)
    {
        _workspaceService = workspaceService;
        _dialogService = dialogService;
        _fileSystemService = fileSystemService;
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

    [RelayCommand]
    public async Task DeleteFileAsync(WorkspaceItemViewModel? item)
    {
        if (item is null || item.IsDirectory)
        {
            return;
        }

        var confirmed = await _dialogService.ConfirmAsync(
            "Delete File",
            $"Are you sure you want to delete \"{item.Name}\"?"
        );
        if(!confirmed)
        {
            return;
        }
        try
        {
            if(!_workspaceService.IsPathWithinWorkspace(item.FullPath))
            {
                throw new PathOutsideWorkspaceException(item.FullPath);
            }

            await _fileSystemService.DeleteFileAsync(item.FullPath);

            RemoveFromTree(item);
            FileDeleted?.Invoke(this, item.FullPath);
        }
        catch(WorkspaceIoException ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }
    }

    private void RemoveFromTree(WorkspaceItemViewModel item)
    {
        if (RootItems.Remove(item))
        {
            return;
        }

        RemoveFromChildren(RootItems, item);
    }

    private static bool RemoveFromChildren(IEnumerable<WorkspaceItemViewModel> nodes, WorkspaceItemViewModel target)
    {
        foreach (var node in nodes)
        {
            if (node.Children.Remove(target))
            {
                return true;
            }

            if (RemoveFromChildren(node.Children, target))
            {
                return true;
            }
        }

        return false;
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
        if(!node.IsTextFile && !node.IsImageFile)
        {
            return;
        }

        SetSyncedSelection(false); 
        _currentlySelected = node;

        FileSelected?.Invoke(this, node);
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

    private static WorkspaceItemViewModel? FindItemByPath(IEnumerable<WorkspaceItemViewModel> items, string filePath)
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