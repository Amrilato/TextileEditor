using SkiaSharp;
using System.Collections;
using Textile.Common;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.Common;

/// <summary>
/// Represents the corners of a rectangular grid.
/// </summary>
public enum Corner
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

/// <summary>
/// Represents a grid index with X and Y coordinates.
/// </summary>
/// <param name="X">The X-coordinate of the grid index.</param>
/// <param name="Y">The Y-coordinate of the grid index.</param>
public readonly record struct GridIndex(int X, int Y)
{
    /// <summary>
    /// Offsets the grid index by a specified grid range.
    /// </summary>
    /// <param name="range">The grid range to offset by.</param>
    /// <returns>A new <see cref="GridIndex"/> adjusted by the range.</returns>
    public GridIndex Offset(GridRange range) => new(X - range.Left, Y - range.Top);

    /// <summary>
    /// Adds two grid indices together.
    /// </summary>
    public static GridIndex operator +(GridIndex left, GridIndex right) => new(left.X + right.X, left.Y + right.Y);

    /// <summary>
    /// Subtracts one grid index from another.
    /// </summary>
    public static GridIndex operator -(GridIndex left, GridIndex right) => new(left.X - right.X, left.Y - right.Y);
}

/// <summary>
/// Represents a rectangular range on a grid.
/// </summary>
/// <param name="x1">The X-coordinate of the first corner.</param>
/// <param name="y1">The Y-coordinate of the first corner.</param>
/// <param name="x2">The X-coordinate of the second corner.</param>
/// <param name="y2">The Y-coordinate of the second corner.</param>
public readonly struct GridRange(int x1, int y1, int x2, int y2) : IEnumerable<GridIndex>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GridRange"/> struct with two grid indices.
    /// </summary>
    public GridRange(GridIndex index1, GridIndex index2) : this(index1.X, index1.Y, index2.X, index2.Y) { }

    /// <summary>
    /// Gets the top-left corner of the range.
    /// </summary>
    public readonly GridIndex First = new(Math.Min(x1, x2), Math.Min(y1, y2));

    /// <summary>
    /// Gets the bottom-right corner of the range.
    /// </summary>
    public readonly GridIndex Second = new(Math.Max(x1, x2), Math.Max(y1, y2));

    public int Top => First.Y;
    public int Bottom => Second.Y;
    public int Left => First.X;
    public int Right => Second.X;
    public int Width => Right - Left;
    public int Height => Bottom - Top;

    /// <summary>
    /// Gets a grid range relative to a specified index and corner.
    /// </summary>
    public GridRange GetRelativeGridRange(GridIndex index, Corner corner)
    {
        int offsetX = 0;
        int offsetY = 0;

        switch (corner)
        {
            case Corner.TopLeft:
                offsetX = index.X - Left;
                offsetY = index.Y - Top;
                break;
            case Corner.TopRight:
                offsetX = index.X - Right;
                offsetY = index.Y - Top;
                break;
            case Corner.BottomLeft:
                offsetX = index.X - Left;
                offsetY = index.Y - Bottom;
                break;
            case Corner.BottomRight:
                offsetX = index.X - Right;
                offsetY = index.Y - Bottom;
                break;
        }

        return new GridRange(Left + offsetX, Top + offsetY, Right + offsetX, Bottom + offsetY);
    }

    /// <summary>
    /// Determines if a specified grid index is inside the range.
    /// </summary>
    public bool IsInsideArea(GridIndex index)
    {
        return index.X >= Left &&
               index.X <= Right &&
               index.Y >= Top &&
               index.Y <= Bottom;
    }

    /// <summary>
    /// Enumerates all grid indices within the range.
    /// </summary>
    public IEnumerator<GridIndex> GetEnumerator()
    {
        for (int x = Left; x <= Right; x++)
        {
            for (int y = Top; y <= Bottom; y++)
            {
                yield return new(x, y);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Represents the settings for a grid layout.
/// </summary>
/// <param name="BorderWidth">The width of the grid borders.</param>
/// <param name="ColumnLength">The number of columns in the grid.</param>
/// <param name="RowLength">The number of rows in the grid.</param>
/// <param name="ColumnWidth">The width of each column in the grid.</param>
/// <param name="RowHeight">The height of each row in the grid.</param>
public readonly record struct GridSettings(int BorderWidth, int ColumnLength, int RowLength, int ColumnWidth, int RowHeight);

/// <summary>
/// Represents the size of a grid, including border width, column width, and row height.
/// </summary>
/// <param name="BorderWidth">The width of the borders.</param>
/// <param name="ColumnWidth">The width of a column.</param>
/// <param name="RowHeight">The height of a row.</param>
public readonly record struct GridSize(int BorderWidth, int ColumnWidth, int RowHeight)
{
    /// <summary>
    /// Attempts to parse a <see cref="GridSize"/> from a string.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <param name="gridSize">The parsed grid size if successful.</param>
    /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
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

/// <summary>
/// Extension methods for <see cref="GridSize"/>.
/// </summary>
public static class GridSizeExtensions
{
    /// <summary>
    /// Converts a <see cref="GridSize"/> to <see cref="GridSettings"/> using textile data.
    /// </summary>
    public static GridSettings ToSettings(this GridSize size, ITextileSize textileData) => new(size.BorderWidth, textileData.Width, textileData.Height, size.ColumnWidth, size.RowHeight);
}


/// <summary>
/// Extension methods for grid settings.
/// </summary>
public static class GridSettingExtensions
{
    /// <summary>
    /// Checks if a specified index is inside the grid.
    /// </summary>
    /// <param name="settings">The grid settings to evaluate.</param>
    /// <param name="index">The grid index to check.</param>
    /// <returns>True if the index is inside the grid; otherwise, false.</returns>
    public static bool IsInsideGrid(this GridSettings settings, GridIndex index) => index.X >= 0 && index.X <= settings.ColumnLength && index.Y >= 0 && index.Y <= settings.RowLength;

    /// <summary>
    /// Calculates the offset for a column border based on the column index.
    /// </summary>
    /// <param name="gridSettings">The grid settings containing column and border dimensions.</param>
    /// <param name="index">The column index.</param>
    /// <returns>The offset of the column border.</returns>
    public static int ColumnBorderOffset(this GridSettings gridSettings, int index) => index * gridSettings.ColumnWidth + index * gridSettings.BorderWidth;

    /// <summary>
    /// Calculates the offset for a row border based on the row index.
    /// </summary>
    /// <param name="gridSettings">The grid settings containing row and border dimensions.</param>
    /// <param name="index">The row index.</param>
    /// <returns>The offset of the row border.</returns>
    public static int RowBorderOffset(this GridSettings gridSettings, int index) => index * gridSettings.RowHeight + index * gridSettings.BorderWidth;

    /// <summary>
    /// Calculates the offset for the grid content area of a column based on the column index.
    /// </summary>
    /// <param name="gridSettings">The grid settings containing column and border dimensions.</param>
    /// <param name="index">The column index.</param>
    /// <returns>The offset of the column grid content area.</returns>
    public static int ColumnGridOffset(this GridSettings gridSettings, int index) => index * gridSettings.ColumnWidth + index * gridSettings.BorderWidth + gridSettings.BorderWidth;

    /// <summary>
    /// Calculates the offset for the grid content area of a row based on the row index.
    /// </summary>
    /// <param name="gridSettings">The grid settings containing row and border dimensions.</param>
    /// <param name="index">The row index.</param>
    /// <returns>The offset of the row grid content area.</returns>
    public static int RowGridOffset(this GridSettings gridSettings, int index) => index * gridSettings.RowHeight + gridSettings.BorderWidth + gridSettings.BorderWidth;

    /// <summary>
    /// Calculates the total height of the grid, including all rows and borders.
    /// </summary>
    /// <param name="gridSettings">The grid settings containing row and border dimensions.</param>
    /// <returns>The total height of the grid.</returns>
    public static int GridHeight(this GridSettings gridSettings) => gridSettings.RowBorderOffset(gridSettings.RowLength) + gridSettings.BorderWidth;

    /// <summary>
    /// Calculates the total width of the grid, including all columns and borders.
    /// </summary>
    /// <param name="gridSettings">The grid settings containing column and border dimensions.</param>
    /// <returns>The total width of the grid.</returns>
    public static int GridWidth(this GridSettings gridSettings) => gridSettings.ColumnBorderOffset(gridSettings.ColumnLength) + gridSettings.BorderWidth;

    /// <summary>
    /// Gets the rectangle representing a cell at a specified index.
    /// </summary>
    /// <param name="gridSettings">The grid settings defining cell and border dimensions.</param>
    /// <param name="index">The grid index of the cell.</param>
    /// <returns>An <see cref="SKRect"/> defining the cell's position and dimensions.</returns>
    public static SKRect GetCellOffset(this GridSettings gridSettings, GridIndex index)
    {
        var top = gridSettings.RowBorderOffset(index.Y) + gridSettings.BorderWidth;
        var left = gridSettings.ColumnBorderOffset(index.X) + gridSettings.BorderWidth;
        return new(left, top, left + gridSettings.ColumnWidth, top + gridSettings.RowHeight);
    }

    /// <summary>
    /// Gets the rectangle representing a cell at a specified textile index.
    /// </summary>
    /// <param name="gridSettings">The grid settings defining cell and border dimensions.</param>
    /// <param name="index">The textile index of the cell.</param>
    /// <returns>An <see cref="SKRect"/> defining the cell's position and dimensions.</returns>
    public static SKRect GetCellOffset(this GridSettings gridSettings, TextileIndex index)
    {
        var top = gridSettings.RowBorderOffset(index.Y) + gridSettings.BorderWidth;
        var left = gridSettings.ColumnBorderOffset(index.X) + gridSettings.BorderWidth;
        return new(left, top, left + gridSettings.ColumnWidth, top + gridSettings.RowHeight);
    }

    /// <summary>
    /// Gets the rectangle representing a range of cells in the grid.
    /// </summary>
    /// <param name="gridSettings">The grid settings defining cell and border dimensions.</param>
    /// <param name="range">The range of cells to calculate.</param>
    /// <returns>An <see cref="SKRect"/> defining the position and dimensions of the range.</returns>
    public static SKRect GetRangeRect(this GridSettings gridSettings, GridRange range) => new(
            gridSettings.ColumnBorderOffset(range.Left),
            gridSettings.RowBorderOffset(range.Top),
            gridSettings.ColumnBorderOffset(range.Right + 1),
            gridSettings.RowBorderOffset(range.Bottom + 1));

    /// <summary>
    /// Calculates the index of a cell based on size and offset.
    /// </summary>
    /// <param name="size">The size of the cell.</param>
    /// <param name="offset">The offset position to calculate.</param>
    /// <param name="borderWidth">The border width between cells.</param>
    /// <returns>The calculated index of the cell.</returns>
    private static int GetIndex(int size, double offset, int borderWidth) => (int)Math.Round(offset / (size + borderWidth), MidpointRounding.ToNegativeInfinity);

    /// <summary>
    /// Gets the index of a cell in the grid based on a point.
    /// </summary>
    /// <param name="gridSettings">The grid settings defining cell and border dimensions.</param>
    /// <param name="point">The point to calculate the index for.</param>
    /// <returns>The <see cref="TextileIndex"/> of the cell at the specified point.</returns>
    public static TextileIndex GetIndex(this GridSettings gridSettings, SKPoint point) => gridSettings.GetIndex((int)point.X, (int)point.Y);

    /// <summary>
    /// Gets the index of a cell in the grid based on X and Y offsets.
    /// </summary>
    /// <param name="gridSettings">The grid settings defining cell and border dimensions.</param>
    /// <param name="offsetX">The X offset to calculate the index for.</param>
    /// <param name="offsetY">The Y offset to calculate the index for.</param>
    /// <returns>The <see cref="TextileIndex"/> of the cell at the specified offsets.</returns>
    public static TextileIndex GetIndex(this GridSettings gridSettings, int offsetX, int offsetY) =>
        new(Math.Min(GetIndex(gridSettings.ColumnWidth, offsetX, gridSettings.BorderWidth), gridSettings.ColumnLength - 1), Math.Min(GetIndex(gridSettings.RowHeight, offsetY, gridSettings.BorderWidth), gridSettings.RowLength - 1));

    /// <summary>
    /// Calculates the size of the canvas required for the grid.
    /// </summary>
    /// <param name="gridSettings">The grid settings defining cell and border dimensions.</param>
    /// <returns>An <see cref="SKSizeI"/> representing the canvas size.</returns>
    public static SKSizeI CanvasSize(this GridSettings gridSettings) => new(gridSettings.GridWidth(), gridSettings.GridHeight());
}
