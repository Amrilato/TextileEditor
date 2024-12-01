using SkiaSharp;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Textile.Data;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters;

internal abstract class TextilePainterContext : IInternalTextileEditorContext, IAsyncDisposable
{
    private int disposed;
    private TextileSession textileSession;

    public TextilePainterContext(TextileSession textileSession)
    {
        this.textileSession = textileSession;
        Session.PropertyChanged += Session_PropertyChanged;
    }

    public abstract bool AlreadyRender { get; }
    public TextileSession Session => textileSession;

    public virtual Task SetSessionAsync(TextileSession session)
    {
        SetSession(session);
        return Task.CompletedTask;
    }
    protected void SetSession(TextileSession session)
    {
        Session.PropertyChanged -= Session_PropertyChanged;
        session.PropertyChanged += Session_PropertyChanged;
        InvokePropertyChanged(ref textileSession, session, nameof(Session));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void InvokePropertyChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new(propertyName));
    }
    protected void InvokePropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new(propertyName));
    protected async void DelayInvokePropertyChangedAsync(Task delayTask, [CallerMemberName] string propertyName = "")
    {
        await delayTask;
        PropertyChanged?.Invoke(this, new(propertyName));
    }


    protected virtual void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(TextileSession.TextileStructure):
                OnTextileStructureChanged(Session.TextileStructure);
                break;
            case nameof(TextileSession.BorderColor):
                OnBorderColorChanged(Session.BorderColor);
                break;
            case nameof(TextileSession.FillColor):
                OnFillColorChanged(Session.FillColor);
                break;
            default:
                break;
        }
    }

    protected virtual void OnTextileStructureChanged(TextileStructure textileStructure) { }
    protected virtual void OnBorderColorChanged(SKColor borderColor) { }
    protected virtual void OnFillColorChanged(SKColor fillColor) { }


    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 0)
        {
            Session.PropertyChanged -= Session_PropertyChanged;
            DisposeCore();
            await DisposeAsyncCore();
        }
    }

    protected virtual void DisposeCore() { }
    protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;
}