using Textile.Interfaces;

namespace Textile.Common;

public static class TextileExtensions
{
    public static int TotalElement(this ITextileSize textileSize) => textileSize.Width * textileSize.Height;
}