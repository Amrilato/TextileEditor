using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Services.TextileSessionStorage;

namespace TextileEditor.Web.Layout;

public partial class UploadDialog : IDialogContentComponent<UploadDialogContent>
{
    [Inject]
    public required IEditorConfigure EditorConfigure { get; init; }
    [Inject]
    public required ITextileSessionStorage Storage { get; init; }
    [Inject]
    public required IMessageService MessageService { get; init; }

    [Parameter]
    public UploadDialogContent Content { get; set; } = default!;

    [CascadingParameter]
    public FluentDialog? Dialog { get; set; }

    private int ProgressPercent = 0;
    private FluentInputFileEventArgs[] Files = [];

    private async Task OnCompletedAsync(IEnumerable<FluentInputFileEventArgs> files)
    {
        Files = files.ToArray();

        foreach (var file in Files)
        {
            if(file is not null && file.LocalFile is not null)
            {
                try
                {
                    await TextileSessionSerializer.DeserializeAsync(Storage, Path.GetFileNameWithoutExtension(file.Name), file.LocalFile.OpenRead(), EditorConfigure);
                }
                catch (Exception e)
                {
                    MessageService.NotifyCenter("Failed parse", e.ToString());
                }
                file.LocalFile.Delete();
            }
        }

        ProgressPercent = 0;
    }
}

public class UploadDialogContent
{
    public string FileName { get; set; } = string.Empty;
    public required string Extensions { get; init; }
}