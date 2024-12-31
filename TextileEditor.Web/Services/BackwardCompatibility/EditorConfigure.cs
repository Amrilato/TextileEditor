using SkiaSharp;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Web.Services.BackwardCompatibility;

public class EditorConfigure(IWebStorage webStorageService)
{
    private SKColor borderColor;
    private SKColor selectBorderColor;
    private SKColor fillColor;
    private SKColor pastePreviewFillColor;
    private Corner tieupPosition;
    private GridSize gridSize;
    private int previewHorizontalRepeat = 1;
    private int previewVerticalRepeat = 1;
    private SKSizeI previewPixelSize = new(1, 1);

    public SKColor BorderColor
    {
        get => borderColor;
        set => InvokePropertyChanged(ref borderColor, value);
    }
    public SKColor SelectBorderColor
    {
        get => selectBorderColor;
        set => InvokePropertyChanged(ref selectBorderColor, value);
    }
    public SKColor FillColor
    {
        get => fillColor;
        set => InvokePropertyChanged(ref fillColor, value);
    }
    public SKColor PastePreviewFillColor
    {
        get => pastePreviewFillColor;
        set => InvokePropertyChanged(ref pastePreviewFillColor, value);
    }
    public Corner TieupPosition
    {
        get => tieupPosition;
        set => InvokePropertyChanged(ref tieupPosition, value);
    }
    public GridSize GridSize
    {
        get => gridSize;
        set => InvokePropertyChanged(ref gridSize, value);
    }
    public int PreviewHorizontalRepeat
    {
        get => previewHorizontalRepeat;
        set => InvokePropertyChanged(ref previewHorizontalRepeat, value);
    }
    public int PreviewVerticalRepeat
    {
        get => previewVerticalRepeat;
        set => InvokePropertyChanged(ref previewVerticalRepeat, value);
    }
    public SKSizeI PreviewPixelSize
    {
        get => previewPixelSize;
        set => InvokePropertyChanged(ref previewPixelSize, value);
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    private void InvokePropertyChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    private const string KeyPrefix = "__EditorConfig__";
    private static string GenerateKey(string key) => $"{KeyPrefix}{key}";
    public async Task LoadSettingsAsync(IAppSettings appSettings)
    {
        if (!SKColor.TryParse(await webStorageService.GetItemAsync(GenerateKey(nameof(BorderColor))), out borderColor))
            borderColor = appSettings.BorderColor;
        if (!SKColor.TryParse(await webStorageService.GetItemAsync(GenerateKey(nameof(SelectBorderColor))), out selectBorderColor))
            selectBorderColor = appSettings.AreaSelectBorderColor;
        if (!SKColor.TryParse(await webStorageService.GetItemAsync(GenerateKey(nameof(FillColor))), out fillColor))
            fillColor = appSettings.IntersectionColor;
        if (!SKColor.TryParse(await webStorageService.GetItemAsync(GenerateKey(nameof(PastePreviewFillColor))), out pastePreviewFillColor))
            pastePreviewFillColor = appSettings.PastPreviewIntersectionColor;
        if (!Enum.TryParse(await webStorageService.GetItemAsync(GenerateKey(nameof(TieupPosition))), false, out tieupPosition))
            tieupPosition = appSettings.TieupPosition;
        if (!GridSize.TryParse(await webStorageService.GetItemAsync(GenerateKey(nameof(GridSize))), out gridSize))
            gridSize = appSettings.GridSize;
        if (!int.TryParse(await webStorageService.GetItemAsync(GenerateKey(nameof(PreviewHorizontalRepeat))), out previewHorizontalRepeat))
            previewHorizontalRepeat = appSettings.RepeatHorizontal;
        if (!int.TryParse(await webStorageService.GetItemAsync(GenerateKey(nameof(PreviewVerticalRepeat))), out previewVerticalRepeat))
            previewVerticalRepeat = appSettings.RepeatVertical;
        if (int.TryParse(await webStorageService.GetItemAsync(GenerateKey($"{nameof(PreviewPixelSize)}Width")), out var pixelSizeWidth) &&
            int.TryParse(await webStorageService.GetItemAsync(GenerateKey($"{nameof(PreviewPixelSize)}Height")), out var pixelSizeHeight))
            previewPixelSize = new(pixelSizeWidth, pixelSizeHeight);
        else
            previewPixelSize = appSettings.PixelSize;
    }

    public async Task SaveSettingsAsync()
    {
        await webStorageService.SetItemAsync(GenerateKey(nameof(BorderColor)), borderColor.ToString());
        await webStorageService.SetItemAsync(GenerateKey(nameof(SelectBorderColor)), selectBorderColor.ToString());
        await webStorageService.SetItemAsync(GenerateKey(nameof(FillColor)), fillColor.ToString());
        await webStorageService.SetItemAsync(GenerateKey(nameof(PastePreviewFillColor)), pastePreviewFillColor.ToString());
        await webStorageService.SetItemAsync(GenerateKey(nameof(TieupPosition)), tieupPosition.ToString());
        await webStorageService.SetItemAsync(GenerateKey(nameof(GridSize)), gridSize.ToString());
        await webStorageService.SetItemAsync(GenerateKey(nameof(PreviewHorizontalRepeat)), previewHorizontalRepeat.ToString());
        await webStorageService.SetItemAsync(GenerateKey(nameof(PreviewVerticalRepeat)), previewVerticalRepeat.ToString());
        await webStorageService.SetItemAsync(GenerateKey($"{nameof(PreviewPixelSize)}Width"), previewPixelSize.Width.ToString());
        await webStorageService.SetItemAsync(GenerateKey($"{nameof(PreviewPixelSize)}Height"), previewPixelSize.Height.ToString());
    }
    public async Task RemoveSettingsAsync()
    {
        await webStorageService.RemoveItemAsync(GenerateKey(nameof(BorderColor)));
        await webStorageService.RemoveItemAsync(GenerateKey(nameof(SelectBorderColor)));
        await webStorageService.RemoveItemAsync(GenerateKey(nameof(FillColor)));
        await webStorageService.RemoveItemAsync(GenerateKey(nameof(PastePreviewFillColor)));
        await webStorageService.RemoveItemAsync(GenerateKey(nameof(TieupPosition)));
        await webStorageService.RemoveItemAsync(GenerateKey(nameof(GridSize)));
        await webStorageService.RemoveItemAsync(GenerateKey(nameof(PreviewHorizontalRepeat)));
        await webStorageService.RemoveItemAsync(GenerateKey(nameof(PreviewVerticalRepeat)));
        await webStorageService.RemoveItemAsync(GenerateKey($"{nameof(PreviewPixelSize)}Width"));
        await webStorageService.RemoveItemAsync(GenerateKey($"{nameof(PreviewPixelSize)}Height"));
    }
}
