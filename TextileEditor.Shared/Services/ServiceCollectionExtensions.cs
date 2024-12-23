using Microsoft.Extensions.DependencyInjection;
using TextileEditor.Shared.Services.Configuration;
using TextileEditor.Shared.View.TextileEditor.Pipeline;
using TextileEditor.Shared.View.TextilePreview.Pipeline;

namespace TextileEditor.Shared.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTextileServices<TDataStorage, TTextileEditorViewRenderPipelineProvider, TTextilePreviewRenderPipelineProvider>(this IServiceCollection serviceDescriptors)
        where TDataStorage : class, IDataStorage
        where TTextileEditorViewRenderPipelineProvider : class, ITextileEditorViewRenderPipelineProvider
        where TTextilePreviewRenderPipelineProvider : class, ITextilePreviewRenderPipelineProvider
    {
        serviceDescriptors
            .AddScoped<IDataStorage, TDataStorage>()
            .AddScoped<ISerializedStorage, SerializedStorage>()
            .AddScoped<ITextileEditorViewRenderPipelineProvider, TTextileEditorViewRenderPipelineProvider>()
            .AddScoped<ITextilePreviewRenderPipelineProvider, TTextilePreviewRenderPipelineProvider>()
            .AddScoped<IAppSettings, LazyAppSettings>();

        return serviceDescriptors;
    }
}
