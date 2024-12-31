using DotNext.Buffers;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Buffers;
using Textile.Data;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Resources;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Layout;

public partial class File
{
    private FluentInputFile? FileUploader = default!;
    private bool open = false;
    private readonly string AllowExtensions = Enum.GetNames<TextileFileExtensions>().Aggregate("", (c, n) => $"{c} {(string.IsNullOrEmpty(n) ? "" : $".{n}")}");

    [Parameter, EditorRequired]
    public ITextileSessionManager? TextileSessionManager { get; set; }

    [Inject]
    public required ILocalizer Localizer { get; init; }
    [Inject]
    public required FileDownloadService FileDownloadService { get; init; }

    [Inject]
    public required IDialogService DialogService { get; init; }

    [Inject]
    public required IMessageService MessageService { get; init; }
    
    [Inject]
    public required ITextileSessionStorage Storage { get; init; }

    [Inject]
    public required IAppSettings AppSettings { get; init; }

    private SaveAsDialogContent SaveAsDialogContent { get; } = new();
    private async Task OpenDialogAsync()
    {
        DialogParameters parameters = new()
        {
            Title = Localizer.GetString(SharedResource.SaveAs),
            PrimaryAction = "Yes",
            SecondaryAction = "No",
            Width = "500px",
            TrapFocus = true,
            Modal = true,
            PreventScroll = true
        };

        await DialogService.ShowDialogAsync<SaveAsDialog>(SaveAsDialogContent, parameters);
        //IDialogReference dialog = await DialogService.ShowDialogAsync<SaveAsDialog>(SaveAsDialogContent, parameters);
        //DialogResult? result = await dialog.Result;


        //if (result.Data is not null)
        //{
        //    SaveAsDialogContent? saveAsDialogContent = result.Data as SaveAsDialogContent;
        //}
        //else
        //{
        //}
    }

    private async Task UploadTextileAsync()
    {
        if(true)//todo: check browser and os, ShowFilesDialogAsync is doesn't work safari and ios
        {

            DialogParameters parameters = new()
            {
                Title = Localizer.GetString(SharedResource.Upload),
                Width = "500px",
                PrimaryAction = Localizer.GetString(SharedResource.Done),
                TrapFocus = true,
                Modal = true,
                PreventScroll = true
            };

            await DialogService.ShowDialogAsync<UploadDialog>(new UploadDialogContent() { Extensions = AllowExtensions }, parameters);
            //IDialogReference dialog = await DialogService.ShowDialogAsync<UploadDialog>(new UploadDialogContent() { Extensions = AllowExtensions }, parameters);
            //DialogResult? result = await dialog.Result;


            //if (result.Data is not null)
            //{
            //    UploadDialogContent? saveAsDialogContent = result.Data as UploadDialogContent;
            //}
            //else
            //{
            //}
        }
        //else
        //{
        //    if (FileUploader is null)
        //        return;
        //    await FileUploader.ShowFilesDialogAsync();
        //}
    }

    private async Task CreateSessionAsync()
    {
        DialogParameters parameters = new()
        {
            Title = Localizer.GetString(SharedResource.CreateNew),
            PrimaryAction = Localizer.GetString(SharedResource.Create),
            TrapFocus = true,
            Modal = true,
            PreventScroll = true
        };

        IDialogReference dialog = await DialogService.ShowDialogAsync<CreateDialog>(new CreateDialogContent(), parameters);
        DialogResult? result = await dialog.Result;


        if (result is not null && !result.Cancelled && result.Data is CreateDialogContent dialogContent)
            _ = Storage.CreateAsync(new TextileStructureSize(dialogContent.TieupWidth, dialogContent.TieupHeight, dialogContent.TextileWidth, dialogContent.TextileHeight), dialogContent.SessionName);

    }

    private async Task DownloadSessionAsync()
    {
        if (TextileSessionManager is null || TextileSessionManager.CurrentSession is null)
            MessageService.NotifyCenter("Textile is not selected", "please select textile", MessageIntent.Warning);
        else
        {
            using PoolingArrayBufferWriter<byte> buffer = new(ArrayPool<byte>.Shared);
            Storage.Serialize(TextileSessionManager.CurrentSession.TextileData, buffer);
            using var memory = buffer.DetachBuffer();
            await FileDownloadService.DownloadAsync(memory.Memory.ToArray(), TextileSessionManager.CurrentSession.TextileData.Name, ".tsd");
        }
    }
    private async void GenerateDownloadLinkAsync()
    {
        if (TextileSessionManager is null || TextileSessionManager.CurrentSession is null)
            MessageService.NotifyCenter("Textile is not selected", "please select textile", MessageIntent.Warning);
        else
        {
            using PoolingArrayBufferWriter<byte> buffer = new(ArrayPool<byte>.Shared);
            Storage.Serialize(TextileSessionManager.CurrentSession.TextileData, buffer);
            using var memory = buffer.DetachBuffer();
            var href = await FileDownloadService.CreateBlobUrlAsync(memory.Memory.ToArray());
            MessageService.ShowMessageBar(options =>
            {
                options.Intent = MessageIntent.Info;
                options.Title = $"{TextileSessionManager.CurrentSession.TextileData.Name} Download Link";
                options.Body = $"right click to link and save as file.";
                options.Link = new() { Href = href, Text = "Download" };
                options.Timestamp = DateTime.Now;
                options.Section = IMessageServiceExtensions.NotificationCenter;
                options.OnClose = e => FileDownloadService.RemoveItemAsync(href).AsTask();
            });
        }
    }
}