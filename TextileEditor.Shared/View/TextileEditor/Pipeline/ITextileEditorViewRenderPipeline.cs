﻿using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextileEditor.Pipeline;

public interface ITextileEditorViewRenderPipeline<TIndex, TValue>
{
    int RenderAsyncPhase { get; }
    int UpdateDifferencesAsyncPhase { get; }
    Task<RenderProgress> RenderAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
    Task<RenderProgress> UpdateDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, ITextileEditorViewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
}
