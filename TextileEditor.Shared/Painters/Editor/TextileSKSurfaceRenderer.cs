using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.Painters.Renderers;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters;

internal abstract class TextileSKSurfaceRenderer<TIntersection, TBorder, TIndex, TValue>(ConcurrencyBackgroundWorkContext concurrencyBackgroundWorkContext, SKSizeI sKSizeI) : AsyncSKSurfaceRenderer(concurrencyBackgroundWorkContext, sKSizeI), ITextileSKSurfaceRenderer<TIndex, TValue>
{
    protected abstract ITextileIntersectionRenderer<TIndex, TValue, TIntersection> GetIntersectionRenderer();
    protected abstract ITextileBorderRenderer<TBorder> GetBorderRenderer();
    protected abstract IReadOnlyTextile<TIndex, TValue> GetTextile();
    protected abstract GridSize GetGridSize();

    public Task InitializedAsync()
    {
        SizeChange(GetGridSize().ToSettings(GetTextile()).CanvasSize());

        ConcurrencyBackgroundWork work = GetWork();
        return Post(() => PrerenderCoreAsync(work), work);
    }
    public Task UpdateAsync(ReadOnlySpan<ChangedValue<TIndex, TValue>> changes)
    {
        var rent = changes.ToRentArray();

        ConcurrencyBackgroundWork work = GetWork();
        return Post(() => PrerenderCoreAsync(work, rent), work);
    }


    private ConcurrencyBackgroundWork GetWork() => CreateWork(GetBorderRenderer().GetMaxStep(GetTextile(), GetGridSize()) + GetIntersectionRenderer().GetMaxStep(GetTextile(), GetGridSize(), GetTextile().Indices));

    private async Task PrerenderCoreAsync(ConcurrencyBackgroundWork work)
    {
        using var surface = CreateSurface(out _);
        await GetBorderRenderer().RenderAsync(surface, work, GetTextile(), GetGridSize(), work.CancellationToken);
        await GetIntersectionRenderer().RenderAsync(surface, work, GetTextile(), GetGridSize(), GetTextile().Indices, work.CancellationToken);
    }

    private async Task PrerenderCoreAsync(ConcurrencyBackgroundWork work, RentArray<ChangedValue<TIndex, TValue>> rentArray)
    {
        try
        {
            using var surface = CreateSurface(out _);
            await GetIntersectionRenderer().RenderAsync(surface, work, GetTextile(), GetGridSize(), rentArray.Values, work.CancellationToken);
        }
        finally
        {
            rentArray.Dispose();
        }
    }
}
