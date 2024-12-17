using Textile.Colors;
using Textile.Common;

namespace Textile.Interfaces;

public interface IReadOnlyTextileStructure
{
    IReadOnlyObservableTextile<TextileIndex, bool> Heddle { get; }
    IReadOnlyObservableTextile<TextileIndex, bool> Pedal { get; }
    IReadOnlyObservableTextile<TextileIndex, bool> Tieup { get; }
    IReadOnlyObservableTextile<TextileIndex, bool> Textile { get; }
    IReadOnlyObservableTextile<int, Color> HeddleColor { get; }
    IReadOnlyObservableTextile<int, Color> PedalColor { get; }
}
