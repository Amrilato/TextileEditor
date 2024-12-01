using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Diagnostics.CodeAnalysis;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Services.TextileSessionStorage;
using TextileEditor.Web.Localization;

namespace TextileEditor.Web.Layout;

public partial class TextileSessionView
{
    [Inject]
    public required IStringLocalizer<SharedResource> Localizer { get; init; }
    [Inject]
    public required ITextileSessionStorage Storage { get; init; }
    [Parameter]
    public TextileSession? Session { get; set; }
    [Parameter]
    public ITextileSessionManager? SessionManager { get; set; }

    private bool renaming = false;
    private string SessionName
    {
        get => Session?.Name ?? "please enter textile session name";
        set
        {
            if (Session is not null)
                Session.Name = value;
            renaming = false;
            StateHasChanged();
        }
    }

    private void Rename() => renaming = !renaming;
    private void Remove()
    {
        if (Session is not null)
            Storage.RemoveAsync(Session);
    }
    [MemberNotNullWhen(true, nameof(Session), nameof(SessionManager))]
    private bool Check => Session is not null && SessionManager is not null;
    private bool IsOpenSession => Check && SessionManager.CurrentSession is not null && Session.Guid == SessionManager.CurrentSession.Guid;
    private async Task OpenSession()
    {
        if (Check)
            await SessionManager.UpdateSessionAsync(Session);
    }
}
