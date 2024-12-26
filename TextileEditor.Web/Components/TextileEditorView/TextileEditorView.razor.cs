using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.ComponentModel;
using TextileEditor.Shared.View.TextileEditor;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Components;

public partial class TextileEditorView : ComponentBase, IDisposable
{
    [Inject]
    public required ILocalizer Localizer { get; init; }

    [Parameter]
    public TextileEditorViewContext? Context { get; set; }
}