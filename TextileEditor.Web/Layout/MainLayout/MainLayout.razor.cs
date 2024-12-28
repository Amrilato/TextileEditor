using Microsoft.AspNetCore.Components;
using TextileEditor.Shared.Services;

namespace TextileEditor.Web.Layout;

public partial class MainLayout : LayoutComponentBase, ITextileSessionManager
{
    [Inject]
    public required IAppSettings AppSettings { get; init; }
    [Inject]
    public required ITextileSessionStorage SessionStorage { get; init; }

    public TextileSession? CurrentSession => TextileSession is null ? null : TextileSession;
    private TextileSession? TextileSession = default;

    public Task UpdateSessionAsync(TextileSession session)
    {
        TextileSession = session;

        StateHasChanged();
        return Task.CompletedTask;
    }

    public void UnsetSession()
    {
        TextileSession = null;
        StateHasChanged();
    }
}
