using Textile.Common;

namespace TextileEditor.Shared.Painters;

internal interface ITextileSKSurfaceRenderer<TIndex, TValue>
{
    bool AlreadyRender { get; }

    Task InitializedAsync();
    Task UpdateAsync(ReadOnlySpan<ChangedValue<TIndex, TValue>> changes);
}