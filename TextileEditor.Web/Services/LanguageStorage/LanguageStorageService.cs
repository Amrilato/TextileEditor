using System.Collections.Immutable;
using System.Globalization;

namespace TextileEditor.Web.Services;

public class LanguageStorageService(IWebStorageService webStorageService, ImmutableArray<CultureInfo> supportedLanguages) : ILanguageStorageService
{
    private const string BlazorCulture = "BlazorCulture";

    public ImmutableArray<CultureInfo> SupportedLanguages { get; } = supportedLanguages;
    private CultureInfo _culture = CultureInfo.InvariantCulture;
    public CultureInfo GetCachedCulture() => _culture;
    public async ValueTask<CultureInfo> GetCulture()
    {
        var culture = await webStorageService.GetItemAsync(BlazorCulture);
        return culture is not null ? (_culture = new CultureInfo(culture)) : CultureInfo.InvariantCulture;
    }
    public async ValueTask SetCulture(CultureInfo culture)
    {
        if (!SupportedLanguages.Contains(culture))
            throw new NotSupportedException();
        await webStorageService.SetItemAsync(BlazorCulture, culture.Name);
        _culture = culture;
    }
}