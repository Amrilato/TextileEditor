namespace TextileEditor.Web.Services;

public class BlazorTextileEnvironmentConfigure(IWebStorageService webStorageService) : IBlazorTextileEnvironmentConfigure
{
    private int threshold;
    private int chunkSize;

    public int Threshold { get => threshold; set => threshold = value; }
    public int ChunkSize { get => chunkSize; set => chunkSize = value; }

    private const string KeyPrefix = "__BlazorTextileEnvironmentConfig__";
    private static string GenerateKey(string key) => $"{KeyPrefix}{key}";
    public async Task LoadSettingsAsync()
    {
        if (!int.TryParse(await webStorageService.GetItemAsync(GenerateKey(nameof(Threshold))), out threshold))
            threshold = 500;
        if (!int.TryParse(await webStorageService.GetItemAsync(GenerateKey(nameof(ChunkSize))), out chunkSize))
            chunkSize = 10;
    }

    public async Task SaveSettingsAsync()
    {
        await webStorageService.SetItemAsync(GenerateKey(nameof(Threshold)), threshold.ToString());
        await webStorageService.SetItemAsync(GenerateKey(nameof(ChunkSize)), chunkSize.ToString());
    }
}
