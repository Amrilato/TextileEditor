using Textile.Colors;
using Textile.Common;

namespace Textile.Interfaces;

public interface IReadOnlyTextileColor : IReadOnlyTextile<int, Color>
{
    int ToIndex(TextileIndex index);
    TextileIndex ToIndex(int accessorIndex, int nonAccessorIndex);
}
