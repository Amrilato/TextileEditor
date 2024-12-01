using Microsoft.Extensions.DependencyInjection;
using TextileEditor.Shared.Services.TextileSessionStorage;

namespace TextileEditor.Shared.Services.Extensions;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddTextileServices<TEditorConfigure, TSessionStorage>(this IServiceCollection serviceDescriptors)
        where TEditorConfigure : class, IEditorConfigure
        where TSessionStorage : class, ITextileSessionStorage
    {
        serviceDescriptors.AddScoped<IBackgroundWorkerService, BackgroundWorkerService>();
        serviceDescriptors.AddScoped<IEditorConfigure, TEditorConfigure>();
        serviceDescriptors.AddScoped<ITextileSessionStorage, TSessionStorage>();

        return serviceDescriptors;
    }
}
