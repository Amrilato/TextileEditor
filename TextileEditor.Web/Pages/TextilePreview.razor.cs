using Microsoft.AspNetCore.Components;
using R3;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.View.TextilePreview;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Pages;

public partial class TextilePreview : IDisposable
{
    [Inject]
    public required ILocalizer Localizer { get; init; }
    [Inject]
    public required IAppSettings AppSettings { get; init; }
    [CascadingParameter(Name = CascadingParameterNames.Session)]
    public TextileSession? Session { get; set; }

    public TextilePreviewContext? TextilePreviewContext => Session?.TextilePreviewContext;

    private int PixelSizeX
    {
        get => AppSettings.PixelSize.Width;
        set => AppSettings.PixelSize = AppSettings.PixelSize with { Width = value };
    }
    private int PixelSizeY
    {
        get => AppSettings.PixelSize.Height;
        set => AppSettings.PixelSize = AppSettings.PixelSize with { Height = value };
    }
    private bool visible = true;
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
                    var prev = visible;
                    visible = s != Shared.View.Common.RenderProgressStates.Completed;
                    if (prev != visible)
                        await InvokeAsync(StateHasChanged);
                }, AwaitOperation.Sequential);
    }

    public void Dispose()
    {
        disposable?.Dispose();
    }
}
