using Textile.Common;

namespace Textile.Data;

public abstract class VerticalTextile(int width, int height) : TextileData(width, height, new uint[height.GetArraySize() * width])
{
    protected override int ArrayConsecutiveLength => Height;
    protected override int ArrayNonConsecutiveLength => Width;
    protected override int ArrayConsecutiveIndex(TextileIndex index) => index.Y;
    protected override int ArrayNonConsecutiveIndex(TextileIndex index) => index.X;
    protected override TextileIndex ToIndex(int ConsecutiveIndex, int NonConsecutiveIndex) => new(NonConsecutiveIndex, ConsecutiveIndex);
    internal override ReadOnlyMemory<uint> HorizontalLine(int index)
    {
        var buffer = new uint[Width.GetArraySize()];
        ArrayNonConsecutiveLine(index, buffer);
        return buffer;
    }
    internal override ReadOnlyMemory<uint> VerticalLine(int index) => ArrayConsecutiveLine(index);
}
