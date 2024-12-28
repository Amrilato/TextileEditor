using Microsoft.AspNetCore.Components;

namespace TextileEditor.Web.Layout;

public partial class Progress
{

    [Parameter]
    public string Name { get; set; } = "";

    [Parameter]
    public string Description { get; set; } = "";

    [Parameter]
    public int ProgressMax { get; set; }

    [Parameter]
    public int ProgressValue { get; set; }
}
