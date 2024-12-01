using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.ComponentModel;
using TextileEditor.Shared.Painters;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Localization;

namespace TextileEditor.Web.Components;

public partial class TextileEditorView : ComponentBase, IDisposable
{
    [Inject]
    public required IStringLocalizer<SharedResource> Localizer { get; init; }
    [Inject]
    public required IBackgroundWorkerService BackgroundWorkerService { get; init; }

    private ITextileEditorContext? PreviousPainters;
    [Parameter]
    public ITextileEditorContext? Painters { get; set; }

    protected override void OnParametersSet()
    {
        if(Painters != PreviousPainters)
        {
            if(Painters is not null)
                Painters.PropertyChanged += Painters_RenderStateChanged;

            if (PreviousPainters is not null)
                PreviousPainters.PropertyChanged -= Painters_RenderStateChanged;

            PreviousPainters = Painters;
        }
    }

    private void Painters_RenderStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(ITextileEditorContext.AlreadyRender))
            InvokeAsync(StateHasChanged);
    }

    private void InvokeStateHasChange()
    {
        StateHasChanged();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (Painters is not null)
            Painters.PropertyChanged -= Painters_RenderStateChanged;
    }
}