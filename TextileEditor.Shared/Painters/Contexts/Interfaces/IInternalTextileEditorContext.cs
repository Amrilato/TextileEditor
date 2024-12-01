using TextileEditor.Shared.Services;

namespace TextileEditor.Shared.Painters;

internal interface IInternalTextileEditorContext : ITextilePainterContext
{
    Task SetSessionAsync(TextileSession textileSession);
}
