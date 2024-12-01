using System.ComponentModel;
using TextileEditor.Shared.Services;

namespace TextileEditor.Shared.Painters;

public interface ITextilePainterContext : INotifyPropertyChanged
{
    bool AlreadyRender { get; }
    TextileSession Session { get; }
}
