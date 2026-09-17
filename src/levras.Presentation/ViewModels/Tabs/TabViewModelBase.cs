using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace levras.Presentation.ViewModels;

public abstract partial class TabViewModelBase : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private bool _isDirty;
    public string FilePath {get; private set;}

    protected TabViewModelBase(string fullPath, string title)
    {
        FilePath = fullPath;
        _title = title;
    }

    public void UpdatePath(string newPath)
    {
        FilePath = newPath;
        Title = Path.GetFileName(newPath);
    }
}