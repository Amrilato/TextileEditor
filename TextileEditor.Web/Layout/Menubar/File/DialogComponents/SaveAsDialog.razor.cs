using R3;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Layout;

public partial class SaveAsDialog : IDialogContentComponent<SaveAsDialogContent>, IDisposable
{
    [Inject]
    public required ILocalizer Localizer { get; init; }

    [Parameter]
    public SaveAsDialogContent Content { get; set; } = default!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    private IDisposable? disposable;
    protected override void OnParametersSet()
    {
        disposable?.Dispose();
        disposable = Localizer.ChangeCulture.Subscribe(c => StateHasChanged());
    }

    private bool PrimaryActionButtonEnable = false;
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender || PrimaryActionButtonEnable != !string.IsNullOrEmpty(Content?.FileName))
            Dialog?.TogglePrimaryActionButton(PrimaryActionButtonEnable = !string.IsNullOrEmpty(Content?.FileName));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        disposable?.Dispose();
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