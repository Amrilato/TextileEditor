﻿using DotNext.Buffers;
using SkiaSharp;
using System.Buffers;
using System.ComponentModel;
using TextileEditor.Shared.Serialization.Configuration;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.Services.Internal;

internal class LazyAppSettings(IDataStorage dataStorage) : IAppSettings
{
    private static async Task<AppSettings> LoadAsync(IDataStorage dataStorage)
    {
        using var owner = await dataStorage.LoadAsync(AppSettingKey);
        try
        {
            return owner is not null ? AppSettingsSerializer.Deserialize(owner.Memory) : Default;
        }
        catch (Exception)
        {
            return Default;
        }
    }
    private static TimeSpan Timeout => TimeSpan.FromSeconds(1);
    private readonly Task<AppSettings> loadTask = LoadAsync(dataStorage);
    private static AppSettings Default => new() { GridSize = new(1, 20, 20), BorderColor = SKColors.Black, IntersectionColor = SKColors.Blue };
    private const string AppSettingKey = nameof(AppSettings);
    private AppSettings? appSettings;
    private AppSettings AppSettings
    {
        get
        {
            if (appSettings is not null)
                return appSettings;
            if (SpinWait.SpinUntil(() => loadTask.IsCompleted, Timeout) && loadTask.IsCompletedSuccessfully)
                return appSettings = loadTask.Result;
            else
                return appSettings = Default;
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
        PoolingArrayBufferWriter<byte> buffer = new(ArrayPool<byte>.Shared);
        AppSettingsSerializer.Serialize(AppSettings, buffer);
        await dataStorage.SaveAsync(AppSettingKey, buffer.DetachBuffer().Span);
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => AppSettings.PropertyChanged += value;
        remove => AppSettings.PropertyChanged -= value;
    }
}