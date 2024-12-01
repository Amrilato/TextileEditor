using MessagePack;
using SkiaSharp;
using Textile.Data;
using TextileEditor.Shared.MessagePackFormatters;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Services;

[MessagePackObject]
public readonly struct TextileSessionDataTransferObject
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

    public TextileSessionDataTransferObject(TextileSession session)
    {
        UseDefaultConfigure = session.UseDefaultConfigure;
        TextileStructure = session.TextileStructure;
        BorderColor = session.BorderColor;
        FillColor = session.FillColor;
        TieupPosition = session.TieupPosition;
    }
}
