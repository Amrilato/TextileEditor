using Textile.Common;

namespace Textile.Data;

public abstract class HorizontalTextile(int width, int height) : TextileData(width, height, new uint[width.GetArraySize() * height])
{
    protected override int ArrayConsecutiveLength => Width;
    protected override int ArrayNonConsecutiveLength => Height;
    protected override int ArrayConsecutiveIndex(TextileIndex index) => index.X;
    protected override int ArrayNonConsecutiveIndex(TextileIndex index) => index.Y;
    protected override TextileIndex ToIndex(int ConsecutiveIndex, int NonConsecutiveIndex) => new(ConsecutiveIndex, NonConsecutiveIndex);
    internal override ReadOnlyMemory<uint> HorizontalLine(int index) => ArrayConsecutiveLine(index);
    internal override ReadOnlyMemory<uint> VerticalLine(int index)
    {
        var buffer = new uint[Height.GetArraySize()];
        ArrayNonConsecutiveLine(index, buffer);
        return buffer;
    }
}
