
using levras.Core.Abstractions;
using levras.Infrastrucutre.FileService;
using Microsoft.Extensions.DependencyInjection;

namespace levras.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IWorkspaceService, WorkspaceService>();
        return services;
    }
}