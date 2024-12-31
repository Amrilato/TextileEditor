using Microsoft.AspNetCore.Components;
using R3;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.View.TextilePreview;

namespace TextileEditor.Web.Components;

public partial class TextilePreview
{
    [Inject]
    public required IAppSettings AppSettings { get; init; }
    [Parameter]
    public TextileSession? Session { get; set; }

    public TextilePreviewContext? TextilePreviewContext => Session?.TextilePreviewContext;

    private IDisposable? disposable;
    protected override void OnParametersSet()
    {
        disposable?.Dispose();
        if (TextilePreviewContext is not null)
            disposable =
                TextilePreviewContext.Painter.RenderingProgress
                .Select(r => r.Status)
                .DistinctUntilChanged().SubscribeAwait(async (s, token) =>
                {
                    if (s == Shared.View.Common.RenderProgressStates.Completed)
                        await InvokeAsync(StateHasChanged);
                }, AwaitOperation.Sequential);
    }

    public void Dispose() => disposable?.Dispose();
}
