﻿using Microsoft.Extensions.Localization;
using System.Collections.Immutable;
using System.Globalization;

namespace TextileEditor.Web.Services;

public class Localizer(IWebStorage webStorage, IStringLocalizer<SharedResource> stringLocalizer, ImmutableArray<CultureInfo> supportedLanguages) : ILocalizer
{
    private const string BlazorCulture = "BlazorCulture";

    public ImmutableArray<CultureInfo> SupportedLanguages { get; } = supportedLanguages;
    public async ValueTask<CultureInfo> GetCulture()
    {
        var culture = await webStorage.GetItemAsync(BlazorCulture);
        try
        {
            return culture is not null ? new CultureInfo(culture) : CultureInfo.InvariantCulture;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async ValueTask SetCulture(CultureInfo culture)
    {
        if (!SupportedLanguages.Contains(culture))
            throw new NotSupportedException();
        if (CultureInfo.DefaultThreadCurrentCulture != culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            ChangeCulture?.Invoke(culture);
        }
        await webStorage.SetItemAsync(BlazorCulture, culture.Name);
    }
    public string GetString(SharedResource key) => stringLocalizer[key.ToString()];
    public event Action<CultureInfo>? ChangeCulture;
}