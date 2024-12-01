using Microsoft.AspNetCore.Components;
using SkiaSharp;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Pages;

public partial class Configure
{
    [Inject]
    public required IBlazorTextileEnvironmentConfigure EnvironmentConfigure { get; init; }
    [Inject]
    public required IEditorConfigure EditorConfigure { get; init; }

    protected override void OnInitialized()
    {
        BorderWidth = EditorConfigure.GridSize.BorderWidth;
        ColumnWidth = EditorConfigure.GridSize.ColumnWidth;
        RowHeight = EditorConfigure.GridSize.RowHeight;
        BorderSKColor = EditorConfigure.BorderColor;
        FillSKColor = EditorConfigure.FillColor;
        PasteFillSKColor = EditorConfigure.PastePreviewFillColor;
    }

    private async Task SaveConfig()
    {
        EditorConfigure.GridSize = new(BorderWidth, ColumnWidth, RowHeight);
        EditorConfigure.BorderColor = BorderSKColor;
        EditorConfigure.FillColor = FillSKColor;
        EditorConfigure.PastePreviewFillColor = PasteFillSKColor;
        var e = EnvironmentConfigure.SaveSettingsAsync();
        var c = EditorConfigure.SaveSettingsAsync();
        await Task.WhenAll(e, c);
    }

    public int BorderWidth { get; set; }
    public int ColumnWidth { get; set; }
    public int RowHeight { get; set; }

    
    private SKColor BorderSKColor { get; set; } = SKColors.White;
    private string BorderColor
    {
        get => BorderSKColor.ToHexColor();
        set
        {
            if (SKColor.TryParse(value, out SKColor color))
                BorderSKColor = color;
            else
                throw new ArgumentException(nameof(value));
        }
    }
    private SKColor FillSKColor { get; set; } = SKColors.White;
    private string FillColor
    {
        get => FillSKColor.ToHexColor();
        set
        {
            if (SKColor.TryParse(value, out SKColor color))
                FillSKColor = color;
            else
                throw new ArgumentException(nameof(value));
        }
    }
    private SKColor PasteFillSKColor { get; set; } = SKColors.White;
    private string PasteFillColor
    {
        get => PasteFillSKColor.ToHexColor();
        set
        {
            if (SKColor.TryParse(value, out SKColor color))
                PasteFillSKColor = color;
            else
                throw new ArgumentException(nameof(value));
        }
    }
}

public static class ColorHelper
{
    public static string ToHexColor(this SKColor color)
    {
        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
    }
}