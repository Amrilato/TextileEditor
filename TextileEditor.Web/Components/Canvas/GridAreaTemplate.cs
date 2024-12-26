using TextileEditor.Shared.View.TextileEditor;

namespace TextileEditor.Shared.Shared.Common;

public readonly record struct GridAreaTemplate(string Textile, string Pedal, string Heddle, string Tieup, string PedalColor, string HeddleColor)
{
    public static GridAreaTemplate GetAreaTemplate(Corner corner) => corner switch
    {
        Corner.TopLeft => TieupTopLeft,
        Corner.TopRight => TieupTopRight,
        Corner.BottomLeft => TieupBottomLeft,
        Corner.BottomRight => TieupBottomRight,
        _ => throw new ArgumentException(nameof(Corner))
    };

    public static Corner GetCorner(GridAreaTemplate gridAreaTemplate)
    {
        if (gridAreaTemplate == TieupTopLeft)
            return Corner.TopLeft;
        else if (gridAreaTemplate == TieupTopRight)
            return Corner.TopRight;
        else if (gridAreaTemplate == TieupBottomLeft)
            return Corner.BottomLeft;
        else if (gridAreaTemplate == TieupBottomRight)
            return Corner.BottomRight;
        else
            throw new ArgumentException(nameof(Corner));
    }

    public static GridAreaTemplate TieupTopLeft => new(BottomRight, BottomCenter, CenterRight, CenterCenter, BottomLeft, TopRight);
    public static GridAreaTemplate TieupTopRight => new(BottomLeft, BottomCenter, CenterLeft, CenterCenter, BottomRight, TopLeft);
    public static GridAreaTemplate TieupBottomLeft => new(TopRight, TopCenter, CenterRight, CenterCenter, TopLeft, BottomRight);
    public static GridAreaTemplate TieupBottomRight => new(TopLeft, TopCenter, CenterLeft, CenterCenter, TopRight, BottomLeft);

    private const string TopLeft = "grid-area: 1 / 1 / 2 / 2;";
    private const string TopCenter = "grid-area: 1 / 2 / 2 / 3;";
    private const string TopRight = "grid-area: 1 / 3 / 2 / 4;";
    private const string CenterLeft = "grid-area: 2 / 1 / 3 / 2;";
    private const string CenterCenter = "grid-area: 2 / 2 / 3 / 3;";
    private const string CenterRight = "grid-area: 2 / 3 / 3 / 4;";
    private const string BottomLeft = "grid-area: 3 / 1 / 4 / 2;";
    private const string BottomCenter = "grid-area: 3 / 2 / 4 / 3;";
    private const string BottomRight = "grid-area: 3 / 3 / 4 / 4;";
}