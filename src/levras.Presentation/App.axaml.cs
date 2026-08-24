using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using levras.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using levras.Presentation.ViewModels;
using levras.Presentation.Views;
using levras.Presentation.Services;

namespace levras.Presentation;

public partial class App : Application
{
    public IServiceProvider Services {get; private set;} = null!;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        
        services.AddInfrastructure();
        services.AddViewModels();
        services.AddPresentaionServices();

        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}