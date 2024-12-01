namespace TextileEditor.Shared.Shared.Common;

public readonly record struct GridIndex(int X, int Y)
{
    public GridIndex Offset(GridRange range) => new(X - range.Left, Y - range.Top);

    public static GridIndex operator +(GridIndex left, GridIndex right) => new(left.X + right.X, left.Y + right.Y);
    public static GridIndex operator -(GridIndex left, GridIndex right) => new(left.X - right.X, left.Y - right.Y);
}
