using SkiaSharp;
using Textile.Colors;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public class TextileColorClickEventHandler : TextileEditorEventHandlerBase<int, Color>, IStatefulTextileEditorEventHandler<int, Color>
{
    private Color _color;
    public SKColor Color
    {
        get => _color.AsSKColor();
        set => _color = value.AsColor();
    }

    private int Index = -1;
    private bool IsPointerDown = false;
    private bool UpdateTextileIndex(int index)
    {
        if (Index == index)
            return false;
        Index = index;
        return true;
    }

    private void SetColor(SKPoint point, ITextile<int, Color> textileData, GridSize size)
    {
        if (textileData is IReadOnlyTextileColor color && UpdateTextileIndex(color.ToIndex(size.ToSettings(textileData).GetIndex(point))))
            textileData[Index] = _color;
    }

    public override void OnClick(SKPoint point, ITextile<int, Color> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        SetColor(point, textileData, configure.GridSize);
    }

    public override void OnPointerLeave(SKPoint point, ITextile<int, Textile.Colors.Color> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        IsPointerDown = false;
        Index = -1;
    }

    public override void OnPointerDown(SKPoint point, ITextile<int, Color> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        IsPointerDown = true;
    }

    public override void OnPointerMove(SKPoint point, ITextile<int, Color> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        if (IsPointerDown)
            SetColor(point, textileData, configure.GridSize);
    }

    public override void OnPointerUp(SKPoint point, ITextile<int, Color> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        if (IsPointerDown)
            SetColor(point, textileData, configure.GridSize);
        IsPointerDown = false;
    }
}
