using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using levras.Core.Abstractions;
using levras.Core.Exceptions;
using levras.Presentation.Services;

namespace levras.Presentation.ViewModels;

public partial class TextEditorTabViewModel : TabViewModelBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly IDialogService _dialogService;
    public TextDocument Document {get; } = new();
    public string MarkdownText => Document.Text;

    private bool _isLoadingContent;

    public TextEditorTabViewModel(
        string filePath,
        IWorkspaceService workspaceService,
        IDialogService dialogService
    ) : base( filePath, Path.GetFileNameWithoutExtension(filePath))
    {
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
        OnPropertyChanged(nameof(MarkdownText));
    }

    public async Task LoadFileAsync(CancellationToken cancellationToken = default )
    {
        _isLoadingContent = true;
        try
        {
            var content = await _workspaceService.ReadFileAsync(FilePath, cancellationToken);
            Document.Text = content;
        }
        finally
        {
            _isLoadingContent = false;
        }
        IsDirty = false;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {   
        try
        {
            await TrySaveInternalAsync();
        }
        catch(WorkspaceIoException ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }
    }

    private async Task<bool> TrySaveInternalAsync()
    {
        if(FilePath is null)
        {
            return false;
        }

        await _workspaceService.WriteFileAsync(FilePath, Document.Text);
        IsDirty = false;
        return true;
    }

    public async Task<bool> TryPrepareToDiscardAsync()
    {
        if(!IsDirty)
        {
            return true;
        }

        var choice = await _dialogService.ConfirmSaveChangesAsync(Path.GetFileName(FilePath));

        return choice switch
        {
            SaveChangesChoice.Save => await TrySaveInternalAsync(),
            SaveChangesChoice.Discard => true,
            SaveChangesChoice.Cancel => false,
            _ => false
        };
    }
}