using SkiaSharp;
using Textile.Colors;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public class TextileColorFillEventHandler : TextileEditorEventHandlerBase<int, Color>, IStatefulTextileEditorEventHandler<int, Color>
{
    private Color _color;
    public SKColor Color
    {
        get => _color.AsSKColor();
        set => _color = value.AsColor();
    }


    public override void OnClick(SKPoint point, ITextile<int, Color> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        if(textileData is IReadOnlyTextileColor color)
        {
            var index = color.ToIndex(configure.GridSize.ToSettings(textileData).GetIndex(point));

            Color c = default;
            var (start, end) = (0, color.Length);
            for (int i = 0; i < color.Length; i++)
            {
                if (color[i] != c)
                {
                    if (start <= index && i >= index)
                    {
                        end = i;
                        break;
                    }
                    else
                    {
                        c = color[i];
                        start = i;
                    }
                }
            }
            textileData.Write(Enumerable.Range(start, end - start).Select(i => new KeyValuePair<int, Color>(i, Color.AsColor())));
        }
    }
}