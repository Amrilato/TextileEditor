using MessagePack;
using Textile.Data;
using TextileEditor.Shared.Common;
using TextileEditor.Shared.Serialization.MessagePackFormatters;

namespace TextileEditor.Shared.Serialization;

[MessagePackObject]
public class TextileData : PropertyChangedBase
{
    public TextileData() => Guid = Guid.NewGuid();

    [Key(0)]
    [MessagePackFormatter(typeof(TextileStructureMessagePackFormatter))]
    public required TextileStructure TextileStructure { get; init; }
    [Key(1)]
    public required string Name
    {
        get => field;
        set => NotifyPropertyChanged(ref field, value);
    }
    [Key(2)]
    public Guid Guid { get; init; }
}
