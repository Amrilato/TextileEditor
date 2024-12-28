using Microsoft.AspNetCore.Components;
using SkiaSharp;
using TextileEditor.Shared.Services;

namespace TextileEditor.Web.Pages;

public partial class Configure
{
    [Inject]
    public required IAppSettings AppSettings { get; init; }

    public int ColumnWidth
    {
        get => AppSettings.GridSize.ColumnWidth;
        set => AppSettings.GridSize = AppSettings.GridSize with { ColumnWidth = value };
    }
    public int RowHeight
    {
        get => AppSettings.GridSize.RowHeight;
        set => AppSettings.GridSize = AppSettings.GridSize with { RowHeight = value };
    }
    private string BorderColor
    {
        get => AppSettings.BorderColor.ToHexColor();
        set
        {
            if (SKColor.TryParse(value, out SKColor color))
                AppSettings.BorderColor = color;
            else
                throw new ArgumentException(nameof(value));
        }
    }
    private string FillColor
    {
        get => AppSettings.IntersectionColor.ToHexColor();
        set
        {
            if (SKColor.TryParse(value, out SKColor color))
                AppSettings.IntersectionColor = color;
            else
                throw new ArgumentException(nameof(value));
        }
    }
    private string PasteFillColor
    {
        get => AppSettings.PastPreviewIntersectionColor.ToHexColor();
        set
        {
            if (SKColor.TryParse(value, out SKColor color))
                AppSettings.PastPreviewIntersectionColor = color;
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