using Microsoft.Extensions.DependencyInjection;

namespace levras.Presentation.ViewModels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<WorkspaceExplorerViewModel>();
        services.AddTransient<TextEditorTabViewModel>();
        services.AddTransient<ImageViewerTabViewModel>();
        return services;
    }
}