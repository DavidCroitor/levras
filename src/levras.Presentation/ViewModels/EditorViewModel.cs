using System;
using System.IO;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using levras.Core.Abstractions;
using levras.Core.Exceptions;
using levras.Presentation.Services;

namespace levras.Presentation.ViewModels;

public partial class EditorViewModel : ViewModelBase
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IWorkspaceService _workspaceService;
    private readonly IDialogService _dialogService;
    public TextDocument Document {get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentFileName))]
    private string? _currentFilePath;
    [ObservableProperty]
    private bool _isDirty;

    private bool _isLoadingContent;

    
    public string CurrentFileName => CurrentFilePath is null ? "Untitled" : Path.GetFileName(CurrentFilePath);

    public EditorViewModel(
        IFileSystemService fileSystemService,
        IWorkspaceService workspaceService,
        IDialogService dialogService
    )
    {
        _fileSystemService = fileSystemService;
        _workspaceService = workspaceService;
        _dialogService = dialogService;

        Document.TextChanged += OnDocumentTextChanged;
    }

    private void OnDocumentTextChanged(object? sender, EventArgs e)
    {
        if(_isLoadingContent)
        {
            return;
        }
        IsDirty = true;
    }

    public async Task LoadFileAsync(string filePath)
    {
        if(!_workspaceService.IsPathWithinWorkspace(filePath))
        {
            throw new PathOutsideWorkspaceException(filePath);
        }

        var content = await _fileSystemService.ReadTextFileAsync(filePath);

        _isLoadingContent = true;

        try
        {
            Document.Text = content;
        }
        finally
        {
            _isLoadingContent = false;
        }

        CurrentFilePath = filePath;
        IsDirty = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await TrySaveInternalAsync();
    }

    private async Task<bool> TrySaveInternalAsync()
    {
        if(CurrentFilePath is null)
        {
            return false;
        }

        if(!_workspaceService.IsPathWithinWorkspace(CurrentFilePath))
        {
            throw new PathOutsideWorkspaceException(CurrentFilePath);
        }

        await _fileSystemService.WriteTextFileAsync(CurrentFilePath, Document.Text);
        IsDirty = false;
        return true;
    }

    public async Task<bool> TryPrepareToDiscardAsync()
    {
        if(!IsDirty)
        {
            return true;
        }

        var choice = await _dialogService.ConfirmSaveChangesAsync(CurrentFileName);

        return choice switch
        {
            SaveChangesChoice.Save => await TrySaveInternalAsync(),
            SaveChangesChoice.Discard => true,
            SaveChangesChoice.Cancel => false,
            _ => false
        };
    }
}