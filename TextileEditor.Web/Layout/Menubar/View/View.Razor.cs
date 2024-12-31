using R3;
using Microsoft.AspNetCore.Components;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Layout;
public partial class View : IDisposable
{
    [Inject]
    public required ILocalizer Localizer { get; init; }

    private IDisposable? disposable;
    protected override void OnParametersSet()
    {
        disposable?.Dispose();
        disposable = Localizer.ChangeCulture.Subscribe(c => StateHasChanged());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        disposable?.Dispose();
    }
}

