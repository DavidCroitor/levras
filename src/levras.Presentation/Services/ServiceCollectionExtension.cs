using Microsoft.Extensions.DependencyInjection;

namespace levras.Presentation.Services;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IFolderPickerService, FolderPickerService>();
        services.AddSingleton<ITabFactory, TabFactory>();
        return services;
    }
}