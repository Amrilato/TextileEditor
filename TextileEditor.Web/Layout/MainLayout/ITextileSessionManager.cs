using TextileEditor.Shared.Services;

namespace TextileEditor.Web.Layout;

public interface ITextileSessionManager
{
    TextileSession? CurrentSession { get; }
    Task UpdateSessionAsync(TextileSession session);
    void UnsetSession();
}
