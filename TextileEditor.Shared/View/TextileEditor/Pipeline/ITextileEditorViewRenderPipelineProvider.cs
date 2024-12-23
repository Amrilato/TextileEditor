using Textile.Colors;
using Textile.Common;

namespace TextileEditor.Shared.View.TextileEditor.Pipeline;

public interface ITextileEditorViewRenderPipelineProvider
{
    ITextileEditorViewRenderPipeline<TextileIndex, bool> CreateTextile();
    ITextileEditorViewRenderPipeline<TextileIndex, bool> CreateHeddle();
    ITextileEditorViewRenderPipeline<TextileIndex, bool> CreatePedal();
    ITextileEditorViewRenderPipeline<TextileIndex, bool> CreateTieup();
    ITextileEditorViewRenderPipeline<int, Color> CreateHeddleColor();
    ITextileEditorViewRenderPipeline<int, Color> CreatePedalColor();
}
