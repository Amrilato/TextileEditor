using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;

namespace TextileEditor.Shared.Shared.Common;

public readonly record struct GridSettings(int BorderWidth, int ColumnLength, int RowLength, int ColumnWidth, int RowHeight);
public readonly record struct GridSize(int BorderWidth, int ColumnWidth, int RowHeight)
{
    public static bool TryParse(string? input, out GridSize gridSize)
    {
        gridSize = default;

        const string prefix = "GridSize { BorderWidth = ";
        const string middle1 = ", ColumnWidth = ";
        const string middle2 = ", RowHeight = ";
        const string suffix = " }";

        if (input is null || !input.StartsWith(prefix) || !input.EndsWith(suffix))
            return false;

        try
        {
            string parameters = input[prefix.Length..^suffix.Length];

            string[] parts = parameters.Split(new string[] { middle1, middle2 }, StringSplitOptions.None);

            if (parts.Length != 3)
            {
                return false;
            }

            if (int.TryParse(parts[0], out int borderWidth) &&
                int.TryParse(parts[1], out int columnWidth) &&
                int.TryParse(parts[2], out int rowHeight))
            {
                gridSize = new GridSize(borderWidth, columnWidth, rowHeight);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}

public static class GridSettingExtensions
{
    public static bool IsInsideGrid(this GridSettings settings, GridIndex index) => index.X >= 0 && index.X <= settings.ColumnLength && index.Y >= 0 && index.Y <= settings.RowLength;

    public static int ColumnBorderOffset(this GridSettings gridSettings, int index) => index * gridSettings.ColumnWidth + index * gridSettings.BorderWidth;
    public static int RowBorderOffset(this GridSettings gridSettings, int index) => index * gridSettings.RowHeight + index * gridSettings.BorderWidth;
    public static int ColumnGridOffset(this GridSettings gridSettings, int index) => index * gridSettings.ColumnWidth + index * gridSettings.BorderWidth + gridSettings.BorderWidth;
    public static int RowGridOffset(this GridSettings gridSettings, int index) => index * gridSettings.RowHeight + index * gridSettings.BorderWidth + gridSettings.BorderWidth;
    public static int GridHeight(this GridSettings gridSettings) => gridSettings.RowBorderOffset(gridSettings.RowLength) + gridSettings.BorderWidth;
    public static int GridWidth(this GridSettings gridSettings) => gridSettings.ColumnBorderOffset(gridSettings.ColumnLength) + gridSettings.BorderWidth;
    public static SKRect GetCellOffset(this GridSettings gridSettings, GridIndex index)
    {
        var top = gridSettings.RowBorderOffset(index.Y) + gridSettings.BorderWidth;
        var left = gridSettings.ColumnBorderOffset(index.X) + gridSettings.BorderWidth;
        return new(left, top, left + gridSettings.ColumnWidth, top + gridSettings.RowHeight);
    }
    public static SKRect GetCellOffset(this GridSettings gridSettings, TextileIndex index)
    {
        var top = gridSettings.RowBorderOffset(index.Y) + gridSettings.BorderWidth;
        var left = gridSettings.ColumnBorderOffset(index.X) + gridSettings.BorderWidth;
        return new(left, top, left + gridSettings.ColumnWidth, top + gridSettings.RowHeight);
    }
    public static SKRect GetRangeRect(this GridSettings gridSettings, GridRange range) => new(
            gridSettings.ColumnBorderOffset(range.Left),
            gridSettings.RowBorderOffset(range.Top),
            gridSettings.ColumnBorderOffset(range.Right + 1),
            gridSettings.RowBorderOffset(range.Bottom + 1));
    private static int GetIndex(int size, double offset, int borderWidth) => (int)Math.Round(offset / (size + borderWidth), MidpointRounding.ToNegativeInfinity);
    public static TextileIndex GetIndex(this GridSettings gridSettings, SKPoint point) => GetIndex(gridSettings, (int)point.X, (int)point.Y);
    public static TextileIndex GetIndex(this GridSettings gridSettings, int offsetX, int offsetY) =>
        new(Math.Min(GetIndex(gridSettings.ColumnWidth, offsetX, gridSettings.BorderWidth), gridSettings.ColumnLength - 1), Math.Min(GetIndex(gridSettings.RowHeight, offsetY, gridSettings.BorderWidth), gridSettings.RowLength - 1));
    public static SKSizeI CanvasSize(this GridSettings gridSettings) => new(gridSettings.GridWidth(), gridSettings.GridHeight());
}

public static class GridSizeExtensions
{
    public static GridSettings ToSettings(this GridSize size, ITextileSize textileData) => new(size.BorderWidth, textileData.Width, textileData.Height, size.ColumnWidth, size.RowHeight);
}