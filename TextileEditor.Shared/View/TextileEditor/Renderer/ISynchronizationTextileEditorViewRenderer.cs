using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextileEditor.Renderer;

internal interface ISynchronizationTextileEditorViewRenderer<TIndex, TValue>
{
    Progress Render(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress);
    Progress UpdateDifferences(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress);
}
