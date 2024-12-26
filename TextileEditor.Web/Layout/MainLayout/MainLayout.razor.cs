using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Painters.Blazor.Renderers;

namespace TextileEditor.Web.Layout;

public partial class MainLayout : ITextileSessionManager
{
    [Inject]
    public required IStringLocalizer<SharedResource> Localizer { get; init; }
    [Inject]
    public required IAppSettings AppSettings { get; init; }
    [Inject]
    public required ITextileSessionStorage SessionStorage { get; init; }

    public TextileSession? CurrentSession => TextileSession is null ? null : TextileSession;
    private TextileSession? TextileSession = default;

    private TextileContextManager? TextileContextManager;

    public Task UpdateSessionAsync(TextileSession session)
    {
        TextileSession = session;

        if(TextileContextManager is null)
        {
            TextileContextManager = new(
                new BlazorReadTextileColorTextileIntersectionRenderer(BlazorTextileEnvironmentConfigure, TextileSession.TextileStructure),
                new BlazorTextileDataIntersectionRenderer(BlazorTextileEnvironmentConfigure, TextileSession.FillColor),
                new BlazorTextileColorIntersectionRenderer(BlazorTextileEnvironmentConfigure, TextileSession.TextileStructure.HeddleColor),
                new BlazorTextileColorIntersectionRenderer(BlazorTextileEnvironmentConfigure, TextileSession.TextileStructure.PedalColor),
                new BlazorTextileBorderRenderer(BlazorTextileEnvironmentConfigure, TextileSession.BorderColor),
                TextileSession,
                BackgroundWorkerService,
                AppSettings);
        }
        else
        {
            TextileContextManager.TextileSession = session;
        }
        StateHasChanged();
        return Task.CompletedTask;
    }

    public void UnsetSession()
    {
        TextileSession = null;
        StateHasChanged();
    }
}
