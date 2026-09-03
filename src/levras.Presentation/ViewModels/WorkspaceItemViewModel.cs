using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using levras.Core.Domain;

namespace levras.Presentation.ViewModels;

public partial class WorkspaceItemViewModel : ViewModelBase
{
    public string Name {get; }
    public string FullPath {get; }
    public bool IsDirectory {get;}
    public bool IsTextFile {get; }
    public bool IsImageFile {get; }
    public WorkspaceItemViewModel? Parent { get; }
    public ObservableCollection<WorkspaceItemViewModel> Children {get; }

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editingName = string.Empty;
    
    internal Action<WorkspaceItemViewModel>? SelectionRequested {get; set;}
    public WorkspaceItemViewModel(WorkspaceItem item, WorkspaceItemViewModel? parent = null)
    {
        Name = item.Name;
        FullPath = item.FullPath;
        IsDirectory = item.IsDirectory;
        Parent = parent;
        Children = new ObservableCollection<WorkspaceItemViewModel> (item.Children.Select(child => new WorkspaceItemViewModel(child, this)));

        IsTextFile = WorkspaceFileTypeClassifier.Classify(item) == WorkspaceFileType.Markdown;
        IsImageFile = WorkspaceFileTypeClassifier.Classify(item) == WorkspaceFileType.Image;
    }
    partial void OnIsSelectedChanged(bool value)
    {
        if (value)
        {
            SelectionRequested?.Invoke(this);
        }
    }
    public void InsertChild(WorkspaceItem item)
    {
        var childVm = new WorkspaceItemViewModel(item); 
        var insertIndex = FindSortedInsertIndex(childVm);
        Children.Insert(insertIndex, childVm);
    }

    public void RemoveChild(string fullPath)
    {
        var target = Children.FirstOrDefault(c => c.FullPath == fullPath);
        if (target is not null) Children.Remove(target);
    }

    public void ReplaceChild(string oldFullPath, WorkspaceItem newItem)
    {
        var index = Children.ToList().FindIndex(c => c.FullPath == oldFullPath);
        if (index < 0) return;
        Children[index] = new WorkspaceItemViewModel(newItem);
    }

    private int FindSortedInsertIndex(WorkspaceItemViewModel item)
    {
        for (int i = 0; i < Children.Count; i++)
        {
            if (!item.IsDirectory && Children[i].IsDirectory) continue;
            if (item.IsDirectory && !Children[i].IsDirectory) return i;
            if (string.Compare(item.Name, Children[i].Name, StringComparison.OrdinalIgnoreCase) < 0) return i;
        }
        return Children.Count;
    }
}