using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TextileEditor.Shared.Painters;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Layout;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Pages;

public partial class Home
{
    [Inject]
    public required IBackgroundWorkerService BackgroundWorker { get; init; }
    [Inject]
    public required FileDownloadService FileDownloadService { get; init; }
    [Inject]
    public required IMessageService MessageService { get; init; }


    [CascadingParameter(Name = CascadingParameterNames.Session)]
    public TextileContextManager? TextileContextManager { get; set; }
    private ConcurrencyBackgroundWorkContext? ConcurrencyBackgroundWorkContext { get; set; }
    private QueueBackgroundWorkContext? QueueBackgroundWorkContext { get; set; }
    private readonly List<string> Strings = ["FirstContent"];
    private void OnColorChanged(ChangeEventArgs e)
    {
        // Handle color change event
        var selectedColor = e.Value?.ToString();
        Console.WriteLine($"Selected Color: {selectedColor}");
    }
    protected override void OnInitialized()
    {
        ConcurrencyBackgroundWorkContext = BackgroundWorker.CreateContext<ConcurrencyBackgroundWorkContext>();
        QueueBackgroundWorkContext = BackgroundWorker.CreateContext<QueueBackgroundWorkContext>();
        ConcurrencyBackgroundWorkContext.Name = "Many Delay (Concurrency)";
        QueueBackgroundWorkContext.Name = "Many Delay (Queue)";
    }

    private async void GenerateDownloadLink()
    {
        if (TextileContextManager is null)
            return;
        var href = await FileDownloadService.CreateBlobUrlAsync(TextileContextManager.TextileEditorContext.Session);
        MessageService.ShowMessageBar(options =>
        {
            options.Intent = MessageIntent.Info;
            options.Title = $"{TextileContextManager.TextileEditorContext.Session.Name} Download Link";
            options.Body = $"right click to link and save as file.";
            options.Link = new() { Href = href, Text = "Download" };
            options.Timestamp = DateTime.Now;
            options.Section = IMessageServiceExtensions.NotificationCenter;
            options.OnClose = e => FileDownloadService.RemoveItemAsync(href).AsTask();
        });
    }


    private async void Working(int delay)
    {
        if (ConcurrencyBackgroundWorkContext is null)
            return;
        var work = ConcurrencyBackgroundWorkContext.Create(10);
        ConcurrencyBackgroundWorkContext.Post(work);
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(delay / 10);
            work.Report(new(1));
        }
        work.Complete();
        Strings.Add($"Number Of {Strings.Count}");
    }

    private void Progress()
    {
        for (int i = 0; i < 3; i++) 
            QueueBackgroundWorkContext?.Post(() => Delay(1000), $"Delay: {3}, Count: {i}");
    }
    private async Task Delay(int delay)
    {
        await Task.Delay(delay).ConfigureAwait(false);
        Strings.Add($"Number Of {Strings.Count}");
        await InvokeAsync(StateHasChanged);
    }
}
