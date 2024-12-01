using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TextileEditor.Shared.Painters;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Localization;

namespace TextileEditor.Web.Pages;

public partial class TextilePreview : IDisposable
{

    [Inject]
    public required IStringLocalizer<SharedResource> Localizer { get; init; }
    [Inject]
    public required IBackgroundWorkerService BackgroundWorkerService { get; init; }

    private TextileContextManager? previousTextileEditorContext;
    [CascadingParameter(Name = CascadingParameterNames.TextileContextManager)]
    public TextileContextManager? TextileEditorContext { get; set; }

    public ITextilePreviewContext? TextilePreviewContext => TextileEditorContext?.TextilePreviewContext;

    private int PixelSizeX
    {
        get => TextilePreviewContext?.PixelSize.Width ?? 1;
        set
        {
            if (TextilePreviewContext is not null)
            {
                TextilePreviewContext.PixelSize = new(value, TextilePreviewContext.PixelSize.Height);
            }
        }
    }
    private int PixelSizeY
    {
        get => TextilePreviewContext?.PixelSize.Height ?? 1;
        set
        {
            if (TextilePreviewContext is not null)
            {
                TextilePreviewContext.PixelSize = new(TextilePreviewContext.PixelSize.Width, value);
            }
        }
    }

    protected override void OnParametersSet()
    {
        if (previousTextileEditorContext != TextileEditorContext)
        {
            if (TextileEditorContext is not null)
            {
                TextileEditorContext.TextilePreviewContext.PropertyChanged += Painters_PropertyChanged;
            }
            if (previousTextileEditorContext is not null)
                previousTextileEditorContext.TextilePreviewContext.PropertyChanged -= Painters_PropertyChanged;
            previousTextileEditorContext = TextileEditorContext;
        }
    }

    private void Painters_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextileEditorPainters.TextileSession):
                {
                    InvokeAsync(StateHasChanged);
                }
                break;
            default:
                break;
        }
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (TextileEditorContext is not null)
            TextileEditorContext.TextilePreviewContext.PropertyChanged += Painters_PropertyChanged;
    }
}
