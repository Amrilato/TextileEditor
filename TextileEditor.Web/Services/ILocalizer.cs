using System.Collections.Immutable;
using System.Globalization;
using TextileEditor.Web.Resources;

namespace TextileEditor.Web.Services
{
    public interface ILocalizer
    {
        ImmutableArray<CultureInfo> SupportedLanguages { get; }

        event Action<CultureInfo>? ChangeCulture;

        ValueTask<CultureInfo> GetCulture();
        string GetString(SharedResource key);
        ValueTask SetCulture(CultureInfo culture);
    }
}