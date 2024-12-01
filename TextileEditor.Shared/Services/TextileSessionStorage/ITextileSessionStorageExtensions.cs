using TextileEditor.Shared.Services.TextileSessionStorage;

namespace TextileEditor.Shared.Services;

public static class ITextileSessionStorageExtensions
{
    public static Guid GenerateGuid(this ITextileSessionStorage storage)
    {
        var id = Guid.NewGuid();
        while (storage.Sessions.Select(s => s.Guid).Contains(id))
            id = Guid.NewGuid();
        return id;
    }
}
