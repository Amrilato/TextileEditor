using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using SkiaSharp;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor.EventHandler;
using TextileEditor.Web.Layout;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Pages;

public partial class TextileEditorPage : IDisposable
{
    private TextileSession? watchSession;

    [Inject]
    public required ILocalizer Localizer { get; init; }
    [Inject]
    public required IMessageService MessageService { get; init; }
    [Inject]
    public required ITextileSessionStorage Storage { get; init; }
    [Inject]
    public required IAppSettings AppSettings { get; init; }
    private readonly IEnumerable<Corner> Corners = Enum.GetValues<Corner>();
    

    [CascadingParameter(Name = CascadingParameterNames.Session)]
    public TextileSession? Session { get; set; }

    private void OnColorChanged(ChangeEventArgs e)
    {
        // Handle color change event
        var selectedColor = e.Value?.ToString();
        if (SKColor.TryParse(selectedColor, out SKColor color) && Session is not null)
        {
            Session.TextileEditorViewContext.TextileEditorColorEventHandler.SetHandler<TextileColorClickEventHandler>().Color = color;
        }

    }

    public void CopyClipboard()
    {
        if (Session is not null)
        {
            Session.TextileEditorViewContext.TextileEditorEventHandler.GetHandler<TextilePasteEventHandler>().Clipboard = Session.TextileEditorViewContext.TextileEditorEventHandler.GetHandler<TextileRangeSelectEventHandler>().Clip();
        }
    }

    private void UpdateSession()
    {
        if (watchSession is not null)
            watchSession.Logger.LoggerStateChanged -= Logger_LoggerStateChanged;
        if(Session is not null)
        {
            watchSession = Session;
            watchSession.Logger.LoggerStateChanged += Logger_LoggerStateChanged;
        }
    }

    private void Logger_LoggerStateChanged() => InvokeAsync(StateHasChanged);

    private void BuildTextileToOther()
    {
        if (Session is null)
            return;

        try
        {
            Session.TextileData.TextileStructure.BuildTextileToOther();
        }
        catch (Exception e)
        {
            MessageService.NotifyCenter("Failed Build", e.ToString());
        }
    }

    private void BuildOtherToTextile()
    {
        if (Session is null)
            return;

        try
        {
            Session.TextileData.TextileStructure.BuildOtherToTextile();
        }
        catch (Exception e)
        {
            MessageService.NotifyCenter("Failed Build", e.ToString());
        }
    }

    private async void SaveSession()
    {
        if (Session is null)
            return;
        await Storage.AddOrSaveAsync(Session);
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (watchSession is not null)
            watchSession.Logger.LoggerStateChanged -= Logger_LoggerStateChanged;
    }
}
