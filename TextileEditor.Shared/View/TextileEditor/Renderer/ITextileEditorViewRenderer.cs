using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextileEditor.Renderer;

public interface ITextileEditorViewRenderer<TIndex, TValue>
{
    Task<RenderProgress> RenderAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, CancellationToken token, IProgress<RenderProgress> progress, RenderProgress currentProgress);
    Task<RenderProgress> UpdateDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, ITextileEditorViewConfigure configure, CancellationToken token, IProgress<RenderProgress> progress, RenderProgress currentProgress);
}
