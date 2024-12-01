using Textile.Common;
using Textile.Interfaces;

namespace Textile.Colors;

public class HeddleColor(int length) : TextileColor(length), ICreateTextileColor<HeddleColor>
{
    public override int Width { get; } = length;
    public override int Height => 1;

    public static HeddleColor Create(int length) => new(length);

    public override int ToIndex(TextileIndex index) => index.X;
    public override TextileIndex ToIndex(int accessorIndex, int nonAccessorIndex) => new(accessorIndex, nonAccessorIndex);
}
