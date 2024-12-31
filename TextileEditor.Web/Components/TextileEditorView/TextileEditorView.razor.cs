using Microsoft.AspNetCore.Components;
using R3;
using System.Diagnostics.CodeAnalysis;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.View.TextileEditor;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Components;

public partial class TextileEditorView : ComponentBase, IDisposable
{
    [Inject]
    public required ILocalizer Localizer { get; init; }

    [Parameter]
    public TextileEditorViewContext? Context { get; set; }
    [Inject]
    public required IAppSettings AppSettings { get; init; }

    private IDisposable? disposableLocalizer;
    private IDisposable? disposable;
    protected override void OnParametersSet()
    {
        disposable?.Dispose();
        if (Context is not null)
            disposable =
                Context.Textile.RenderingProgress
                .Merge(Context.Heddle.RenderingProgress)
                .Merge(Context.Pedal.RenderingProgress)
                .Merge(Context.Tieup.RenderingProgress)
                .Merge(Context.HeddleColor.RenderingProgress)
                .Merge(Context.PedalColor.RenderingProgress)
                .Where(r => r.Status == Shared.View.Common.RenderProgressStates.Completed)
                .Select(r => AlreadyRender)
                .DistinctUntilChanged().SubscribeAwait(async (s, token) =>
                {
                    if (AlreadyRender)
                    {
                        visible = false;
                        await InvokeAsync(StateHasChanged);
                    }
                }, AwaitOperation.Sequential);

        disposableLocalizer?.Dispose();
        disposableLocalizer = Localizer.ChangeCulture.Subscribe(c => StateHasChanged());
    }
    [MemberNotNullWhen(true, nameof(Context))]
    private bool AlreadyRender => Context is not null
        && Context.Textile.RenderingProgress.CurrentValue.Status == Shared.View.Common.RenderProgressStates.Completed
        && Context.Heddle.RenderingProgress.CurrentValue.Status == Shared.View.Common.RenderProgressStates.Completed
        && Context.Pedal.RenderingProgress.CurrentValue.Status == Shared.View.Common.RenderProgressStates.Completed
        && Context.Tieup.RenderingProgress.CurrentValue.Status == Shared.View.Common.RenderProgressStates.Completed
        && Context.HeddleColor.RenderingProgress.CurrentValue.Status == Shared.View.Common.RenderProgressStates.Completed
        && Context.PedalColor.RenderingProgress.CurrentValue.Status == Shared.View.Common.RenderProgressStates.Completed;
    private bool visible = true;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        disposable?.Dispose();
        disposableLocalizer?.Dispose();
    }
}