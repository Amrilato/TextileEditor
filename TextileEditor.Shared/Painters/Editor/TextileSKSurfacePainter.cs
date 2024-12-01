using Textile.Interfaces;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Painters.Renderers;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters;


internal class TextileSKSurfacePainter<TIntersection, TBorder, TIndex, TValue>(ITextileIntersectionRenderer<TIndex, TValue, TIntersection> intersectionRenderer,
                               ITextileBorderRenderer<TBorder> borderRenderer,
                               IReadOnlyTextile<TIndex, TValue> textileData,
                               GridSize gridSize,
                               ConcurrencyBackgroundWorkContext backgroundWorkContext) : TextileSKSurfaceRenderer<TIntersection, TBorder, TIndex, TValue>(backgroundWorkContext, gridSize.ToSettings(textileData).CanvasSize())
{
    protected override IReadOnlyTextile<TIndex, TValue> GetTextile() => TextileData;
    protected override ITextileIntersectionRenderer<TIndex, TValue, TIntersection> GetIntersectionRenderer() => IntersectionRenderer;
    protected override ITextileBorderRenderer<TBorder> GetBorderRenderer() => BorderRenderer;
    protected override GridSize GetGridSize() => GridSize;

    public IReadOnlyTextile<TIndex, TValue> TextileData { get; set; } = textileData;
    public ITextileIntersectionRenderer<TIndex, TValue, TIntersection> IntersectionRenderer { get; set; } = intersectionRenderer;
    public ITextileBorderRenderer<TBorder> BorderRenderer { get; set; } = borderRenderer;
    public GridSize GridSize { get; set; } = gridSize;
}
