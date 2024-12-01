using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;
using SkiaSharp;
using TextileEditor.Shared.Painters;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Services.TextileSessionStorage;
using TextileEditor.Shared.Shared.Common;
using TextileEditor.Web.Layout;
using TextileEditor.Web.Localization;

namespace TextileEditor.Web.Pages;

public partial class TextileEditorPage : IDisposable
{
    private TextileSession? watchSession;

    [Inject]
    public required IStringLocalizer<SharedResource> Localizer { get; init; }
    [Inject]
    public required IMessageService MessageService { get; init; }
    [Inject]
    public required ITextileSessionStorage Storage { get; init; }
    [Inject]
    public required IEditorConfigure EditorConfigure { get; init; }
    private readonly IEnumerable<Corner> Corners = Enum.GetValues<Corner>();


    private TextileContextManager? previousTextileContextManager;
    [CascadingParameter(Name = CascadingParameterNames.TextileContextManager)]
    public TextileContextManager? TextileContextManager { get; set; }

    private void OnColorChanged(ChangeEventArgs e)
    {
        // Handle color change event
        var selectedColor = e.Value?.ToString();
        if (SKColor.TryParse(selectedColor, out SKColor color))
            TextileContextManager?.SetTextileColorClickValue(color);
    }

    protected override void OnParametersSet()
    {
        if (previousTextileContextManager != TextileContextManager)
        {
            if (TextileContextManager is not null)
            {
                TextileContextManager.TextileEditorContext.PropertyChanged += Painters_PropertyChanged;
                UpdateSession();
            }
            if (previousTextileContextManager is not null)
                previousTextileContextManager.TextileEditorContext.PropertyChanged -= Painters_PropertyChanged;
            previousTextileContextManager = TextileContextManager;
        }
    }

    private void UpdateSession()
    {
        if (watchSession is not null)
            watchSession.Logger.LoggerStateChanged -= Logger_LoggerStateChanged;
        if(TextileContextManager is not null)
        {
            watchSession = TextileContextManager.TextileEditorContext.Session;
            watchSession.Logger.LoggerStateChanged += Logger_LoggerStateChanged;
        }
    }

    private void Logger_LoggerStateChanged() => InvokeAsync(StateHasChanged);

    private void Painters_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextileEditorContext.Session):
                UpdateSession();
                break;
            default:
                break;
        }
    }

    private void BuildTextileToOther()
    {
        if (TextileContextManager is null)
            return;

        try
        {
            TextileContextManager.TextileEditorContext.Session.TextileStructure.BuildTextileToOther();
        }
        catch (Exception e)
        {
            MessageService.NotifyCenter("Failed Build", e.ToString());
        }
    }

    private void BuildOtherToTextile()
    {
        if (TextileContextManager is null)
            return;

        try
        {
            TextileContextManager.TextileEditorContext.Session.TextileStructure.BuildOtherToTextile();
        }
        catch (Exception e)
        {
            MessageService.NotifyCenter("Failed Build", e.ToString());
        }
    }

    private async void SaveSession()
    {
        if (TextileContextManager is null)
            return;
        await Storage.AddOrSaveAsync(TextileContextManager.TextileEditorContext.Session);
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (TextileContextManager is not null)
            TextileContextManager.TextileEditorContext.PropertyChanged += Painters_PropertyChanged;
        if (watchSession is not null)
            watchSession.Logger.LoggerStateChanged -= Logger_LoggerStateChanged;
    }
}
