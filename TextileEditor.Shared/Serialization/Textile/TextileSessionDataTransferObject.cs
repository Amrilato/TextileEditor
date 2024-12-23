using MessagePack;
using SkiaSharp;
using Textile.Data;
using TextileEditor.Shared.Serialization.MessagePackFormatters;
using TextileEditor.Shared.View.TextileEditor;

namespace TextileEditor.Shared.Serialization.Textile;

[MessagePackObject(AllowPrivate = true)]
internal readonly struct TextileSessionDataTransferObject
{
    [Key(0)]
    public readonly bool UseDefaultConfigure;
    [Key(1)]
    [MessagePackFormatter(typeof(TextileStructureMessagePackFormatter))]
    public readonly TextileStructure TextileStructure;
    [Key(2)]
    [MessagePackFormatter(typeof(SKColorMessagePackFormatter))]
    public readonly SKColor BorderColor;
    [Key(3)]
    [MessagePackFormatter(typeof(SKColorMessagePackFormatter))]
    public readonly SKColor FillColor;
    [Key(4)]
    [MessagePackFormatter(typeof(CornerMessagePackFormatter))]
    public readonly Corner TieupPosition;

    public TextileSessionDataTransferObject(bool useDefaultConfigure, TextileStructure textileStructure, SKColor borderColor, SKColor fillColor, Corner tieupPosition)
    {
        UseDefaultConfigure = useDefaultConfigure;
        TextileStructure = textileStructure ?? throw new ArgumentNullException(nameof(textileStructure));
        BorderColor = borderColor;
        FillColor = fillColor;
        TieupPosition = tieupPosition;
    }
}
