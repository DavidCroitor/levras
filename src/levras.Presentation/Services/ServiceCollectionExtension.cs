using Microsoft.Extensions.DependencyInjection;

namespace levras.Presentation.Services;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddPresentaionServices(this ServiceCollection services)
    {
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IFolderPickerService, FolderPickerService>();
        return services;
    }
}