using MessagePack;
using SkiaSharp;
using TextileEditor.Shared.Common;
using TextileEditor.Shared.Serialization.MessagePackFormatters;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.Serialization.Configuration;

[MessagePackObject(AllowPrivate = true)]
internal class AppSettings : PropertyChangedBase
{
    [Key(0)]
    [MessagePackFormatter(typeof(GridSizeMessagePackFormatter))]
    public GridSize GridSize
    {
        get => field;
        set => NotifyPropertyChanged(ref field, value);
    }

    [Key(1)]
    [MessagePackFormatter(typeof(SKColorMessagePackFormatter))]
    public SKColor BorderColor
    {
        get => field;
        set => NotifyPropertyChanged(ref field, value);
    }

    [Key(2)]
    [MessagePackFormatter(typeof(SKColorMessagePackFormatter))]
    public SKColor AreaSelectBorderColor
    {
        get => field;
        set => NotifyPropertyChanged(ref field, value);
    }

    [Key(3)]
    [MessagePackFormatter(typeof(SKColorMessagePackFormatter))]
    public SKColor IntersectionColor
    {
        get => field;
        set => NotifyPropertyChanged(ref field, value);
    }

    [Key(4)]
    [MessagePackFormatter(typeof(SKColorMessagePackFormatter))]
    public SKColor PastPreviewIntersectionColor
    {
        get => field;
        set => NotifyPropertyChanged(ref field, value);
    }

    [Key(5)]
    [MessagePackFormatter(typeof(CornerMessagePackFormatter))]
    public Corner TieupPosition
    {
        get => field;
        set => NotifyPropertyChanged(ref field, value);
    }
    [Key(6)]
    public int RepeatVertical
    {
        get => field;
        set => NotifyPropertyChanged(ref field, value);
    }
    [Key(7)]
    public int RepeatHorizontal
    {
        get => field;
        set => NotifyPropertyChanged(ref field, value);
    }
    [Key(8)]
    [MessagePackFormatter(typeof(SKSizeIMessagePackFormatter))]
    public SKSizeI PixelSize
    {
        get => field;
        set => NotifyPropertyChanged(ref field, value);
    }
}
