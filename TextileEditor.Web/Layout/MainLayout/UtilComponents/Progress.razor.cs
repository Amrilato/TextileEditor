using Microsoft.AspNetCore.Components;
using TextileEditor.Shared.Services;

namespace TextileEditor.Web.Layout;

public partial class Progress
{

    [Parameter]
    public string Name { get; set; } = "";

    [Parameter]
    public string Description { get; set; } = "";

    [Parameter]
    public BackgroundTaskProgress ProgressValue { get; set; }
}
