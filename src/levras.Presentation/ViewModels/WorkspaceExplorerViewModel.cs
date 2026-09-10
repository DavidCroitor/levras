using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using levras.Core.Abstractions;
using levras.Core.Domain;
using levras.Core.Exceptions;
using levras.Presentation.Services;

namespace levras.Presentation.ViewModels;

public partial class WorkspaceExplorerViewModel : ViewModelBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly IDialogService _dialogService;
    [ObservableProperty] private WorkspaceItemViewModel? _selectedItem;
    [ObservableProperty] private bool _isWorkspaceOpen = false;
    [ObservableProperty] private string _workspaceTitle = string.Empty;
    private WorkspaceItemViewModel? _pendingCreate;

    public ObservableCollection<WorkspaceItemViewModel> RootItems { get; } = new();

    public event EventHandler<WorkspaceItemViewModel>? FileSelected;
    public event EventHandler<string>? NodeDeleted;
    public event EventHandler<(string oldPath, string newPath)>? NodePathChanged;
    [ObservableProperty]
    private GridLength _width = new(240);

    public double MinWidth { get; } = 200; 
    public double MaxWidth { get; } = 650;

    public WorkspaceExplorerViewModel(
        IWorkspaceService workspaceService,
        IDialogService dialogService)
    {
        _workspaceService = workspaceService;
        _dialogService = dialogService;
    }

    public async Task LoadWorkspaceAsync(string folderPath)
    {
        _workspaceService.OpenWorkspace(folderPath);

        WorkspaceTitle = new DirectoryInfo(_workspaceService.CurrentWorkspacePath!).Name;

        var tree = await _workspaceService.GetWorkspaceTreeAsync();

        RootItems.Clear();
        SelectedItem = null;
        foreach (var item in tree)
        {
            var vm = new WorkspaceItemViewModel(item);
            RootItems.Add(vm);
        }
        IsWorkspaceOpen = true;
    }
    [RelayCommand]
    private async Task MoveNodeAsync((WorkspaceItemViewModel Source, WorkspaceItemViewModel? Target) args)
    {
        var (source, target) = args;
        var destinationFolder = ResolveDestinationFolder(target);
 
        if (!CanMoveNode(source, target)) return;
 
        var destinationPath = destinationFolder?.FullPath ?? _workspaceService.CurrentWorkspacePath!;
 
        try
        {
            if (!_workspaceService.IsPathWithinWorkspace(source.FullPath))
            {
                throw new PathOutsideWorkspaceException(source.FullPath);
            }
 
            var moved = await _workspaceService.MoveAsync(source.FullPath, destinationPath);
 
            var oldPath = source.FullPath;
            RemoveFromTree(source);
            InsertNode(destinationFolder, moved);
 
            NodePathChanged?.Invoke(this, (oldPath, moved.FullPath));
        }
        catch (WorkspaceIoException ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }
    }
 
    /// <summary>
    /// Whether <paramref name="source"/> may be dropped onto <paramref name="target"/>.
    /// A null target means "drop at the workspace root".
    /// </summary>
    public bool CanMoveNode(WorkspaceItemViewModel source, WorkspaceItemViewModel? target)
    {
        if (source == target) return false;
 
        var destinationFolder = ResolveDestinationFolder(target);
 
        // Already there - dropping back onto its current parent is a no-op.
        if (destinationFolder == source.Parent) return false;
 
        // Can't move a folder into itself or one of its own descendants.
        if (source.IsDirectory && (destinationFolder == source || IsDescendantOf(source, destinationFolder)))
        {
            return false;
        }
 
        return true;
    }

    [RelayCommand]
    public async Task DeleteNodeAsync(WorkspaceItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        var confirmed = await _dialogService.ConfirmAsync(
            item.IsDirectory ?   "Delete Folder" : "Delete File",
            item.IsDirectory ?  $"Are you sure you want to delete \"{item.Name}\" and everything inside?" :
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

            await _workspaceService.DeleteAsync(item.FullPath);
            
            RemoveFromTree(item);
            NodeDeleted?.Invoke(this, item.FullPath);
        }
        catch(WorkspaceIoException ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }

    }
    public void SelectByPath(string filePath)
    {
        var node = FindNode(RootItems, filePath);
        if (node is null || node == SelectedItem) return;

        SelectedItem = node;
    }


    // ==================== PRIVATE ====================
    private static int GetSortedInsertIndex(
        IReadOnlyList<WorkspaceItemViewModel> siblings, bool isDirectory, string name)
    {
        for (var i = 0; i < siblings.Count; i++)
        {
            if (CompareForSort(isDirectory, name, siblings[i].IsDirectory, siblings[i].Name) < 0)
            {
                return i;
            }
        }
        return siblings.Count;
    }
 
    private static int CompareForSort(bool aIsDirectory, string aName, bool bIsDirectory, string bName)
    {
        if (aIsDirectory != bIsDirectory)
        {
            return aIsDirectory ? -1 : 1; // directories sort before files
        }
        return string.Compare(aName, bName, StringComparison.OrdinalIgnoreCase);
    }
    private static WorkspaceItemViewModel? ResolveDestinationFolder(WorkspaceItemViewModel? target) =>
    target is null ? null : target.IsDirectory ? target : target.Parent;
 
    private static bool IsDescendantOf(WorkspaceItemViewModel ancestor, WorkspaceItemViewModel? candidate)
    {
        for (var current = candidate; current is not null; current = current.Parent)
        {
            if (current == ancestor) return true;
        }
        return false;
    }
 
    private void InsertNode(WorkspaceItemViewModel? parent, WorkspaceItem item)
    {
        var vm = new WorkspaceItemViewModel(item, parent);
        var siblings = parent?.Children ?? RootItems;
        var index = GetSortedInsertIndex(siblings, item.IsDirectory, item.Name);
        siblings.Insert(index, vm);
 
        if (parent is not null)
        {
            parent.IsExpanded = true;
        }
    }
    partial void OnSelectedItemChanged(WorkspaceItemViewModel? oldValue, WorkspaceItemViewModel? newValue)
    {
        if(oldValue is not null)
        {
            oldValue.IsSelected = false;
        }
        if(newValue is null)
        {
            return;
        }
        newValue.IsSelected = true;

        if (newValue.IsDirectory)
        {
            return;
        }

        if (!newValue.IsTextFile && !newValue.IsImageFile)
        {
            return;
        }

        FileSelected?.Invoke(this, newValue);

    }
    [RelayCommand]
    private void BeginCreateFolder(WorkspaceItemViewModel? targetFolder)
    {
        if(_pendingCreate is not null) return;

        var parent = targetFolder?.IsDirectory == true ? targetFolder : targetFolder?.Parent;
        var placeholder = new WorkspaceItemViewModel(
            new WorkspaceItem(string.Empty, string.Empty, true, []), parent)
        {
            IsEditing = true,
            EditingName = string.Empty,
        };

        if(parent is null)
        {
            RootItems.Insert(0, placeholder);
        }
        else
        {
            parent.Children.Insert(0, placeholder);
        }

        _pendingCreate = placeholder;
    }
    [RelayCommand]
    private void BeginCreateFile(WorkspaceItemViewModel? targetFolder)
    {
        if(_pendingCreate is not null) return;
        var parent = targetFolder?.IsDirectory == true ? targetFolder : targetFolder?.Parent;
        var placeholder = new WorkspaceItemViewModel(
            new WorkspaceItem(string.Empty, string.Empty, false, []), parent)
        {
            IsEditing = true,
            EditingName = string.Empty,
        };

        if(parent is null)
        {
            RootItems.Insert(0, placeholder);
        }
        else
        {
            parent.Children.Insert(0, placeholder);
        }

        _pendingCreate = placeholder;
    }
    [RelayCommand]
    private void BeginRename(WorkspaceItemViewModel node)
    {
        node.EditingName = node.Name;
        node.IsEditing = true;
    }
    [RelayCommand]
    private async Task CommitRenameAsync(WorkspaceItemViewModel node)
    {
        node.IsEditing = false;
        if (node == _pendingCreate)
        {
            _pendingCreate = null;
            if (string.IsNullOrWhiteSpace(node.EditingName)) 
            { 
                RemoveFromTree(node); 
                return; 
            }

            var parentPath = node.Parent?.FullPath ?? _workspaceService.CurrentWorkspacePath!;
            try
            {
                var created = node.IsDirectory
                ? await _workspaceService.CreateFolderAsync(parentPath, node.EditingName)
                : await _workspaceService.CreateFileAsync(parentPath, node.EditingName);
                ReplaceInParent(node, created);
            }
            catch (WorkspaceIoException ex)
            {
                RemoveFromTree(node);
                await _dialogService.ShowErrorAsync(ex.Message);
            }

            return;
        }

        if(node.EditingName == node.Name || string.IsNullOrWhiteSpace(node.EditingName))
        {
            return;
        }

        try
        {
            var renamed = await _workspaceService.RenameAsync(node.FullPath, node.EditingName);
            ReplaceInParent(node, renamed);
            NodePathChanged?.Invoke(this, (node.FullPath, renamed.FullPath));
        }
        catch(WorkspaceIoException ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }

    }
    [RelayCommand]
    private void CancelRename(WorkspaceItemViewModel node)
    {
        node.IsEditing = false;
        if(node == _pendingCreate)
        {
            _pendingCreate = null;
            RemoveFromTree(node);
        }
    }
    private void ReplaceInParent(WorkspaceItemViewModel oldNode, WorkspaceItem newItem)
    {
        var siblings = oldNode.Parent?.Children ?? RootItems;
        siblings.Remove(oldNode);
 
        var newVm = new WorkspaceItemViewModel(newItem, oldNode.Parent);
        var index = GetSortedInsertIndex(siblings, newItem.IsDirectory, newItem.Name);
        siblings.Insert(index, newVm);
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
    private static WorkspaceItemViewModel? FindNode(IEnumerable<WorkspaceItemViewModel> items, string filePath)
    {
        foreach (var item in items)
        {
            if (!item.IsDirectory && item.FullPath == filePath) return item;
            if (item.IsDirectory)
            {
                var found = FindNode(item.Children, filePath);
                if (found is not null) return found;
            }
        }
        return null;
    }
}