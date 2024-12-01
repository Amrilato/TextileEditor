
namespace TextileEditor.Web.Services;

public interface IBlazorTextileEnvironmentConfigure
{
    int Threshold { get; set; }
    int ChunkSize { get; set; }

    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
}