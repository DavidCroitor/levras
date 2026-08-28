using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using levras.Core.Exceptions;
using levras.Presentation.Services;
using Microsoft.VisualBasic;

namespace levras.Presentation.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IFolderPickerService _folderPickerService;
    public WorkspaceExplorerViewModel Explorer {get; }
    public EditorViewModel Editor {get;}
    private readonly IDialogService _dialogService;

    public MainViewModel(
        IFolderPickerService folderPickerService,
        WorkspaceExplorerViewModel explorer,
        EditorViewModel editor,
        IDialogService dialogService
    )
    {
        _dialogService = dialogService;
        _folderPickerService = folderPickerService;
        Explorer = explorer;
        Editor = editor;

        Explorer.FileSelected += OnFileSelected;
        Explorer.FileDeleted += OnFileDeleted;
    }

    private void OnFileDeleted(object? sender, string filePath)
    {
        if(filePath == Editor.CurrentFilePath)
        {
            Editor.Reset();
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
    

    private async void OnFileSelected(object? sender, string filePath)
    {
        try
        {
                if(filePath == Editor.CurrentFilePath)
            {
                return;
            }
            if(!await Editor.TryPrepareToDiscardAsync())
            {
                Explorer.RevertSelectionTo(Editor.CurrentFilePath);
                return;
            }
        }
        catch(WorkspaceIoException ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }

        try
        {
            await Editor.LoadFileAsync(filePath);
        }
        catch (WorkspaceIoException ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }
    }
}
