using CommunityToolkit.Mvvm.ComponentModel;

namespace levras.Presentation.ViewModels;

public abstract partial class TabViewModelBase : ObservableObject
{
    [ObservableProperty] private string _title;
    public string FilePath {get;}

    protected TabViewModelBase(string fullPath, string title)
    {
        FilePath = fullPath;
        _title = title;
        
    }
}