using SkiaSharp;
using System.Drawing;
using Textile.Common;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public class TextileClickEventHandler : TextileEditorEventHandlerBase<TextileIndex, bool>, IStatefulTextileEditorEventHandler<TextileIndex, bool>
{
    private TextileIndex TextileIndex = new(-1, -1);
    private bool IsPointerDown = false;
    private bool IsPointerMoved = false;
    private bool UpdateTextileIndex(TextileIndex index)
    {
        if (TextileIndex == index)
            return false;
        TextileIndex = index;
        return true;
    }

    public override void OnPointerDown(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        IsPointerDown = true;
    }

    public override void OnPointerLeave(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        IsPointerDown = false;
        IsPointerMoved = false;
        TextileIndex = new(-1, -1);
    }

    public override void OnPointerMove(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        if (IsPointerDown)
        {
            IsPointerMoved = true;
            if (UpdateTextileIndex(configure.GridSize.ToSettings(textileData).GetIndex(point)))
                textileData[TextileIndex] = !textileData[TextileIndex];
        }
    }

    public override void OnPointerUp(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        if (IsPointerDown)
        {
            if (!IsPointerMoved)
            {
                UpdateTextileIndex(configure.GridSize.ToSettings(textileData).GetIndex(point));
                textileData[TextileIndex] = !textileData[TextileIndex];
            }
            else
                IsPointerMoved = false;
            IsPointerDown = false;
        }
    }
}
