using System.Collections;

namespace TextileEditor.Shared.Shared.Common;

public readonly struct GridRange(int x1, int y1, int x2, int y2) : IEnumerable<GridIndex>
{
    public GridRange(GridIndex index1, GridIndex index2) : this(index1.X, index1.Y, index2.X, index2.Y) { }

    public readonly GridIndex First = new(Math.Min(x1, x2), Math.Min(y1, y2));
    public readonly GridIndex Second = new(Math.Max(x1, x2), Math.Max(y1, y2));
    public int Top => First.Y;
    public int Bottom => Second.Y;
    public int Left => First.X;
    public int Right => Second.X;
    public int Width => Right - Left;
    public int Height => Bottom - Top;
    public GridRange FirstAt(GridIndex index) => GetRelativeGridRange(index, Corner.TopLeft);
    public bool IsInsideArea(GridIndex index)
    {
        return index.X >= Left &&
               index.X <= Right &&
               index.Y >= Top &&
               index.Y <= Bottom;
    }
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
