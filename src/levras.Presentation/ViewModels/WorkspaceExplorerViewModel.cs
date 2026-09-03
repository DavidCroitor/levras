using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
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
    private readonly IFileSystemService _fileSystemService;
    [ObservableProperty] private WorkspaceItemViewModel? _selectedItem;
    [ObservableProperty] private bool _isWorkspaceOpen = false;
    private WorkspaceItemViewModel? _pendingCreate;

    public ObservableCollection<WorkspaceItemViewModel> RootItems { get; } = new();

    public event EventHandler<WorkspaceItemViewModel>? FileSelected;
    public event EventHandler<string>? FileDeleted;
    public event EventHandler<(string oldPath, string newPath)>? NodePathChanged;

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
        SelectedItem = null;
        foreach (var item in tree)
        {
            var vm = new WorkspaceItemViewModel(item);
            RootItems.Add(vm);
        }
        IsWorkspaceOpen = true;
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
    public void SelectByPath(string filePath)
    {
        var node = FindNode(RootItems, filePath);
        if (node is null || node == SelectedItem) return;

        SelectedItem = node;
    }
    
    private void InsertSorted(ObservableCollection<WorkspaceItemViewModel> children, WorkspaceItemViewModel newVm)
    {
        for (var index = 0; index < children.Count; index++)
        {
            var existing = children[index];
            if (!newVm.IsDirectory && existing.IsDirectory)
            {
                continue;
            }

            if (newVm.IsDirectory && !existing.IsDirectory ||
                string.Compare(newVm.Name, existing.Name, StringComparison.OrdinalIgnoreCase) < 0)
            {
                children.Insert(index, newVm);
                return;
            }
        }

        children.Add(newVm);
    }

    // ==================== PRIVATE ====================
    partial void OnSelectedItemChanged(WorkspaceItemViewModel? value)
    {
        if(value is null) return;

        if(value.IsDirectory) return;
        if(!value.IsTextFile && !value.IsImageFile) return;

        FileSelected?.Invoke(this, value);

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
                var created = await _workspaceService.CreateFileAsync(parentPath, node.EditingName);
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
        catch(WorkspaceIoFailureException ex)
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
        var newVm = new WorkspaceItemViewModel(newItem, oldNode.Parent);
        if(oldNode.Parent is null)
        {
            var index = RootItems.IndexOf(oldNode);
            RootItems[index] = newVm;
        }
        else
        {
            var index = oldNode.Parent.Children.IndexOf(oldNode);
            oldNode.Parent.Children[index] = newVm;
        }
        
    }
    private void InsertIntoTree(WorkspaceItemViewModel? parentVm, WorkspaceItem? newItem)
    {
        var newVm = new WorkspaceItemViewModel(newItem!);
        if(parentVm == null)
        {
            RootItems.Add(newVm);
        }
        else
        {
            parentVm.InsertChild(newItem!);
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