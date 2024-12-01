using Textile.Interfaces;

namespace Textile.Common;

public readonly record struct ChangedValue<TIndex, TValue>(TIndex Index, TValue Current, TValue Previous);

public delegate void TextileStateChangedEventHandler<TIndex, TValue>(IReadOnlyTextile<TIndex, TValue> sender, TextileStateChangedEventArgs<TIndex, TValue> eventArgs);
public readonly ref struct TextileStateChangedEventArgs<TIndex, TValue>(ReadOnlySpan<ChangedValue<TIndex, TValue>> changedIndices)
{
    public readonly ReadOnlySpan<ChangedValue<TIndex, TValue>> ChangedIndices = changedIndices;
}