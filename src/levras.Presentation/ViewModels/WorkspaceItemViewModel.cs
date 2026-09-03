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

        var fileType = WorkspaceFileTypeClassifier.Classify(item);
        IsTextFile = fileType == WorkspaceFileType.Markdown;
        IsImageFile = fileType == WorkspaceFileType.Image;
    }
    partial void OnIsSelectedChanged(bool value)
    {
        if (value)
        {
            SelectionRequested?.Invoke(this);
        }
    }

}