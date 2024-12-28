using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Buffers;
using TextileEditor.Shared.Services;

namespace TextileEditor.Web.Layout;

public partial class UploadDialog : IDialogContentComponent<UploadDialogContent>
{
    [Inject]
    public required IStringLocalizer<SharedResource> Localizer { get; init; }
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
                    var name = Path.GetFileNameWithoutExtension(file.Name);
                    var stream = file.LocalFile.OpenRead();
                    var buffer = ArrayPool<byte>.Shared.Rent(checked((int)stream.Length));
                    var data = buffer[..checked((int)stream.Length)];
                    await stream.ReadExactlyAsync(data);
                    await Storage.DeserializeAsync(data);
                }
                catch (Exception e)
                {
                    MessageService.NotifyCenter("Failed parse", e.ToString());
                }
                finally
                {
                    file.LocalFile.Delete();
                }
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