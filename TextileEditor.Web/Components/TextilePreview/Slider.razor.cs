using Microsoft.AspNetCore.Components;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Components;

public partial class Slider
{
    [Inject]
    public required ILocalizer Localizer { get; init; }
    [Inject]
    public required IAppSettings AppSettings { get; init; }

    private int PixelSizeX
    {
        get => AppSettings.PixelSize.Width;
        set => AppSettings.PixelSize = AppSettings.PixelSize with { Width = value };
    }
    private int PixelSizeY
    {
        get => AppSettings.PixelSize.Height;
        set => AppSettings.PixelSize = AppSettings.PixelSize with { Height = value };
    }
}
