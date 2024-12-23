using Textile.Colors;
using Textile.Data;
using Textile.Interfaces;

namespace Textile.Common;

public static class TextileExtensions
{
    public static int TotalElement(this ITextileSize textileSize) => textileSize.Width * textileSize.Height;
    public static TextileIndex ToIndex(this IReadOnlyTextile<int, Color> textile, int accessorIndex, int nonAccessorIndex)
    {
        if (textile is IReadOnlyTextileColor c)
            return c.ToIndex(accessorIndex, nonAccessorIndex);
        else if (textile is TextileBase d)
            return d.ToIndex(accessorIndex, nonAccessorIndex);
        else
            throw new NotImplementedException();
    }
}