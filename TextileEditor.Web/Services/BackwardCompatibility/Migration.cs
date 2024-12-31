using TextileEditor.Shared.Services;

namespace TextileEditor.Web.Services.BackwardCompatibility;

public static class Migration
{
    public static async Task Migration001to010(EditorConfigure editorConfigure, OldTextileSessionStorage oldTextileSessionStorage, IAppSettings appSettings, ITextileSessionStorage textileSessionStorage)
    {
        await editorConfigure.LoadSettingsAsync(appSettings);
        await editorConfigure.RemoveSettingsAsync();
        appSettings.GridSize = editorConfigure.GridSize;
        appSettings.TieupPosition = editorConfigure.TieupPosition;
        appSettings.IntersectionColor = editorConfigure.FillColor;
        appSettings.BorderColor = editorConfigure.BorderColor;
        appSettings.AreaSelectBorderColor = editorConfigure.SelectBorderColor;
        appSettings.PastPreviewIntersectionColor = editorConfigure.PastePreviewFillColor;
        appSettings.PixelSize = editorConfigure.PreviewPixelSize;
        appSettings.RepeatHorizontal = editorConfigure.PreviewHorizontalRepeat;
        appSettings.RepeatVertical = editorConfigure.PreviewVerticalRepeat;


        var list = await oldTextileSessionStorage.ReadStorage();
        foreach (var data in list)
        {
            await textileSessionStorage.CreateAsync(data);
        }
    }
}
