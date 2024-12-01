using Microsoft.FluentUI.AspNetCore.Components;

namespace TextileEditor.Web.Layout;

public static class IMessageServiceExtensions
{
    public const string NotificationCenter = nameof(NotificationCenter);

    public static void NotifyCenter(this IMessageService messageService, string title, string message, MessageIntent intent = MessageIntent.Info, ActionLink<Message>? link = null)
    {
        messageService.ShowMessageBar(options =>
        {
            options.Intent = intent;
            options.Title = title;
            options.Body = message;
            options.Link = link;
            options.Timestamp = DateTime.Now;
            options.Section = NotificationCenter;
        });
    }
}
