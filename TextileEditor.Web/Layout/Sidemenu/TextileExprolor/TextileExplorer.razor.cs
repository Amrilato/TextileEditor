using Microsoft.AspNetCore.Components;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Layout;

public partial class TextileExplorer : IDisposable
{
    [Inject]
    public required ILocalizer Localizer { get; init; }
    [Inject]
    public required ITextileSessionStorage Storage { get; init; }
    [Parameter]
    public ITextileSessionManager? SessionManager { get; set; }
    private TextileSession? SelectedSession { get; set; }

    protected override void OnInitialized() => Storage.SessionListChanged += OnSessionListChanged;
    private void OnSessionListChanged(ITextileSessionStorage storage, SessionListChangedEventArgs eventArgs) => InvokeAsync(StateHasChanged);
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Storage.SessionListChanged -= OnSessionListChanged;
    }
}
