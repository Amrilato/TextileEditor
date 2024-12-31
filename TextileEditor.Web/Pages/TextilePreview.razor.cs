using Microsoft.AspNetCore.Components;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Pages;

public partial class TextilePreview
{
    [Inject]
    public required ILocalizer Localizer { get; init; }
    [CascadingParameter(Name = CascadingParameterNames.Session)]
    public TextileSession? Session { get; set; }
}
