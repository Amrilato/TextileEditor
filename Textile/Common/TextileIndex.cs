namespace Textile.Common;

public readonly record struct TextileIndex(int X, int Y)
{
    public static TextileIndex operator +(TextileIndex left, TextileIndex right) => new(left.X + right.X, left.Y + right.Y);
    public static TextileIndex operator -(TextileIndex left, TextileIndex right) => new(left.X - right.X, left.Y - right.Y);
}
