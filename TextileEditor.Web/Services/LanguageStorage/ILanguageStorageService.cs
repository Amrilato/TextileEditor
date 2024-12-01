using System.Collections.Immutable;
using System.Globalization;

namespace TextileEditor.Web.Services;

public interface ILanguageStorageService
{
    ImmutableArray<CultureInfo> SupportedLanguages { get; }
    CultureInfo GetCachedCulture();
    ValueTask<CultureInfo> GetCulture();
    ValueTask SetCulture(CultureInfo culture);
}