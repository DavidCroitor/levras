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
    public ObservableCollection<WorkspaceItemViewModel> Children {get; }

    [ObservableProperty]
    private bool _isSelected;
    [ObservableProperty]
    private bool _isExpanded;
    
    internal Action<WorkspaceItemViewModel>? SelectionRequested {get; set;}
    public WorkspaceItemViewModel(WorkspaceItem item)
    {
        Name = item.Name;
        FullPath = item.FullPath;
        IsDirectory = item.IsDirectory;
        Children = new ObservableCollection<WorkspaceItemViewModel> (item.Children.Select(child => new WorkspaceItemViewModel(child)));

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
}