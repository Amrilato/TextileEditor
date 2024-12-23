using SkiaSharp;
using System.ComponentModel;
using TextileEditor.Shared.View.TextileEditor;

namespace TextileEditor.Shared.Services.Configuration;

internal class LazyAppSettings(ISerializedStorage serializedStorage) : IAppSettings
{
    private static TimeSpan Timeout => TimeSpan.FromSeconds(1);
    private const string AppSettingKey = nameof(AppSettings);
    private readonly Task<AppSettings?> loadTask = serializedStorage.LoadAsync<AppSettings>(AppSettingKey);
    private AppSettings? appSettings;
    private AppSettings AppSettings
    {
        get
        {
            if (appSettings is not null)
                return appSettings;
            if (SpinWait.SpinUntil(() => loadTask.IsCompleted, Timeout) && loadTask.IsCompletedSuccessfully)
                return appSettings = loadTask.Result ?? new();
            else
                return appSettings = new();
        }
    }

    public GridSize GridSize { get => AppSettings.GridSize; set => AppSettings.GridSize = value; }
    public SKColor BorderColor { get => AppSettings.BorderColor; set => AppSettings.BorderColor = value; }
    public SKColor AreaSelectBorderColor { get => AppSettings.AreaSelectBorderColor; set => AppSettings.AreaSelectBorderColor = value; }
    public SKColor IntersectionColor { get => AppSettings.IntersectionColor; set => AppSettings.IntersectionColor = value; }
    public SKColor PastPreviewIntersectionColor { get => AppSettings.PastPreviewIntersectionColor; set => AppSettings.PastPreviewIntersectionColor = value; }
    public Corner TieupPosition { get => AppSettings.TieupPosition; set => AppSettings.TieupPosition = value; }
    public int RepeatVertical { get => AppSettings.RepeatVertical; set => AppSettings.RepeatVertical = value; }
    public int RepeatHorizontal { get => AppSettings.RepeatHorizontal; set => AppSettings.RepeatHorizontal = value; }
    public SKSizeI PixelSize { get => AppSettings.PixelSize; set => AppSettings.PixelSize = value; }

    public async Task SaveAsync()
    {
        if (appSettings is null)
            return;
        await serializedStorage.SaveAsync(AppSettingKey, appSettings);
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => AppSettings.PropertyChanged += value;
        remove => AppSettings.PropertyChanged -= value;
    }
}