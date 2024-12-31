using R3;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Layout;

public partial class TextileSessionView : IDisposable
{
    [Inject]
    public required ILocalizer Localizer { get; init; }
    [Inject]
    public required ITextileSessionStorage Storage { get; init; }
    [Parameter]
    public TextileSession? Session { get; set; }
    [Parameter]
    public ITextileSessionManager? SessionManager { get; set; }

    private IDisposable? disposable;
    protected override void OnParametersSet()
    {
        disposable?.Dispose();
        disposable = Localizer.ChangeCulture.Subscribe(c => StateHasChanged());
    }


    private bool renaming = false;
    private string SessionName
    {
        get => Session?.TextileData.Name ?? "please enter textile session name";
        set
        {
            if (Session is not null)
                Session.TextileData.Name = value;
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
    private bool IsOpenSession => Check && SessionManager.CurrentSession is not null && Session.TextileData.Guid == SessionManager.CurrentSession.TextileData.Guid;
    private async Task OpenSession()
    {
        if (Check)
            await SessionManager.UpdateSessionAsync(Session);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        disposable?.Dispose();
    }
}
