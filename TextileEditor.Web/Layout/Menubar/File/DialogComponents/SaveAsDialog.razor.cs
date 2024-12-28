using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Layout;

public partial class SaveAsDialog : IDialogContentComponent<SaveAsDialogContent>
{
    [Inject]
    public required ILocalizer Localizer { get; init; }

    [Parameter]
    public SaveAsDialogContent Content { get; set; } = default!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    private bool PrimaryActionButtonEnable = false;
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender || PrimaryActionButtonEnable != !string.IsNullOrEmpty(Content?.FileName))
            Dialog?.TogglePrimaryActionButton(PrimaryActionButtonEnable = !string.IsNullOrEmpty(Content?.FileName));
    }
}

public class SaveAsDialogContent
{
    public string FileName { get; set; } = string.Empty;
    public TextileFileExtensions Extension { get; set; } = TextileFileExtensions.tsd;
}
public enum TextileFileExtensions
{
    tsd
}