using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Textile.Data;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Layout;

public partial class CreateDialog : IDialogContentComponent<CreateDialogContent>
{
    [Inject]
    public required ILocalizer Localizer { get; init; }

    [Parameter]
    public CreateDialogContent Content { get; set; } = default!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    private bool PrimaryActionButtonEnable = false;
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender || PrimaryActionButtonEnable != !string.IsNullOrEmpty(Content?.SessionName))
            Dialog?.TogglePrimaryActionButton(PrimaryActionButtonEnable = !string.IsNullOrEmpty(Content?.SessionName));
    }
}

public class CreateDialogContent : ITextileStructureSize
{
    public string SessionName { get; set; } = "New Textile";

    public int TieupWidth { get; set; } = 4;
    public int TieupHeight { get; set; } = 4;
    public int TextileWidth { get; set; } = 20;
    public int TextileHeight { get; set; } = 20;
}