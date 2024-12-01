using Textile.Common;
using Textile.Interfaces;

namespace Textile.Colors;

public class PedalColor(int length) : TextileColor(length), ICreateTextileColor<PedalColor>
{
    public override int Width => 1;
    public override int Height { get; } = length;

    public static PedalColor Create(int length) => new(length);

    public override int ToIndex(TextileIndex index) => index.Y;
    public override TextileIndex ToIndex(int accessorIndex, int nonAccessorIndex) => new(nonAccessorIndex, accessorIndex);
}