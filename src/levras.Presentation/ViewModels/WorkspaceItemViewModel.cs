using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using levras.Core.Models;

namespace levras.Presentation.ViewModels;

public partial class WorkspaceItemViewModel : ViewModelBase
{
    public string Name {get; }
    public string FullPath {get; }
    public bool IsDirectory {get;}
    public IReadOnlyList<WorkspaceItemViewModel> Children {get; }

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
        Children = item.Children.Select(child => new WorkspaceItemViewModel(child)).ToList();
    }
    partial void OnIsSelectedChanged(bool value)
    {
        if (value)
        {
            SelectionRequested?.Invoke(this);
        }
    }
}