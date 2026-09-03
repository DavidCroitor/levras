using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using levras.Core.Exceptions;
using levras.Presentation.Services;
using Microsoft.VisualBasic;

namespace levras.Presentation.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ITabFactory _tabFactory;
    private readonly IDialogService _dialogService;
    private readonly IFolderPickerService _folderPickerService;
    public WorkspaceExplorerViewModel Explorer { get; }
    public ObservableCollection<TabViewModelBase> OpenTabs { get; } = new();
    [ObservableProperty] private TabViewModelBase? _selectedTab;

    public MainViewModel(
            WorkspaceExplorerViewModel explorer,
            IDialogService dialogService, 
            IFolderPickerService folderPickerService,
            ITabFactory tabFactory)
    {
        Explorer = explorer;
        _tabFactory = tabFactory;
        _dialogService = dialogService;
        _folderPickerService = folderPickerService;
        Explorer.FileSelected += OnFileSelected;
        Explorer.FileDeleted += OnFileDeleted;
        Explorer.NodePathChanged += OnNodePathChanged;
    }

    private void OnNodePathChanged(object? sender, (string oldPath, string newPath) change)
    {
        foreach(var tab in OpenTabs)
        {
            if(tab.FilePath == change.oldPath)
            {
                tab.UpdatePath(change.newPath);
            }
            else if (tab.FilePath.StartsWith(change.oldPath + Path.DirectorySeparatorChar))
            {
                var relative = tab.FilePath[(change.oldPath.Length)..];
                tab.UpdatePath(change.newPath + relative);
            }
        }
    }

    private void OnFileDeleted(object? sender, string deletedPath)
    {
        var tab = OpenTabs.FirstOrDefault(t => t.FilePath == deletedPath);
        if (tab is null) return;
        OpenTabs.Remove(tab);
        if (SelectedTab == tab) SelectedTab = OpenTabs.LastOrDefault();
    }

    private async void OnFileSelected(object? sender, WorkspaceItemViewModel node)
    {
        var existing = OpenTabs.FirstOrDefault(t => t.FilePath == node.FullPath);
        if (existing is not null) { SelectedTab = existing; return; }

        var tab = await _tabFactory.CreateTabAsync(node);
        OpenTabs.Add(tab);
        SelectedTab = tab;
    }
    partial void OnSelectedTabChanged(TabViewModelBase? value)
    {
        if(value is not null)
        {
            // Debug.WriteLine(value.FilePath);
            Explorer.SelectByPath(value.FilePath);
        }
    }

    [RelayCommand]
    private async Task OpenWorkspaceAsync()
    {
        try
        {
            var folderPath = await _folderPickerService.PickFolderAsync();
            if (folderPath is null)
            {
                return;
            }
            await Explorer.LoadWorkspaceAsync(folderPath);
        }
        catch(WorkspaceIoException ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }
    }
    [RelayCommand]
    private async Task CloseTabAsync(TabViewModelBase tab)
    {
        if (tab is TextEditorTabViewModel { IsDirty: true } editorTab && !await editorTab.TryPrepareToDiscardAsync())
            return;

        OpenTabs.Remove(tab);
        if (SelectedTab == tab) SelectedTab = OpenTabs.LastOrDefault();
    }   

    [RelayCommand]
    private async Task SaveActiveTabAsync()
    {
        if (SelectedTab is TextEditorTabViewModel textTab)
            await textTab.SaveAsync();
    }
}
