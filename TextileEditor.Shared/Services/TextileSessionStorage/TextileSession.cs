using SkiaSharp;
using System.ComponentModel;
using Textile.Data;
using TextileEditor.Shared.Common;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Services;

public class TextileSession : INotifyPropertyChanged, IDisposable
{
    private readonly IEditorConfigure editorConfigure;
    private bool useDefaultConfigure;
    private TextileStructure textileStructure;
    private SKColor borderColor;
    private SKColor fillColor;
    private Corner tieupPosition;
    private string name;

    internal TextileSession(IEditorConfigure editorConfigure, TextileSessionDataTransferObject textileSessionDataTransferObject, string name, Guid guid) : this(editorConfigure, textileSessionDataTransferObject.UseDefaultConfigure, textileSessionDataTransferObject.TextileStructure, textileSessionDataTransferObject.BorderColor, textileSessionDataTransferObject.FillColor, textileSessionDataTransferObject.TieupPosition, name, guid)
    {

    }
    public TextileSession(IEditorConfigure editorConfigure,
                          bool useDefaultConfigure,
                          TextileStructure textileStructure,
                          SKColor borderColor,
                          SKColor fillColor,
                          Corner tieupPosition,
                          string name,
                          Guid guid)
    {
        this.editorConfigure = editorConfigure ?? throw new ArgumentNullException(nameof(editorConfigure));
        this.useDefaultConfigure = useDefaultConfigure;
        this.textileStructure = textileStructure ?? throw new ArgumentNullException(nameof(textileStructure));
        this.borderColor = borderColor;
        this.fillColor = fillColor;
        this.tieupPosition = tieupPosition;
        this.name = name;
        Guid = guid;

        Logger = new(this);

        editorConfigure.PropertyChanged += EditorConfigurePropertyChanged;
    }

    public TextileLogger Logger { get; }
    public bool UseDefaultConfigure
    {
        get => useDefaultConfigure;
        set
        {
            if (value == useDefaultConfigure)
                return;
            useDefaultConfigure = value;
            if (value)
            {
                borderColor = editorConfigure.BorderColor;
                fillColor = editorConfigure.FillColor;
                tieupPosition = editorConfigure.TieupPosition;
            }
        }
    }
    public TextileStructure TextileStructure
    {
        get => textileStructure;
        set => SetTextileStructure(value);
    }
    protected virtual void SetTextileStructure(TextileStructure textileStructure) => InvokePropertyChanged(ref  this.textileStructure, textileStructure);
    public SKColor BorderColor
    {
        get => useDefaultConfigure ? editorConfigure.BorderColor : borderColor;
        set => SetBorderColor(value);
    }
    protected virtual void SetBorderColor(SKColor value) => InvokePropertyChanged(ref borderColor, value);
    public SKColor FillColor
    {
        get => useDefaultConfigure ? editorConfigure.FillColor : fillColor;
        set => SetFillColor(value);
    }
    protected virtual void SetFillColor(SKColor value) => InvokePropertyChanged(ref fillColor, value);
    public Corner TieupPosition
    {
        get => useDefaultConfigure ? editorConfigure.TieupPosition : tieupPosition;
        set => SetTieupPosition(value);
    }
    protected virtual void SetTieupPosition(Corner value) => InvokePropertyChanged(ref tieupPosition, value);
    public string Name
    {
        get => name;
        set => SetName(value);
    }
    protected virtual void SetName(string value) => InvokePropertyChanged(ref name, value);
    public Guid Guid { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void InvokePropertyChanged(ref TextileStructure field, TextileStructure value, string propertyName = "")
    {
        if (field == value)
            return;
        field = value;
        PropertyChanged?.Invoke(this, new(propertyName));
    }
    private void InvokePropertyChanged(ref Corner field, Corner value, string propertyName = "")
    {
        if (field == value)
            return;
        field = value;
        PropertyChanged?.Invoke(this, new(propertyName));
    }
    private void InvokePropertyChanged<T>(ref T field, T value, string propertyName = "")
        where T : IEquatable<T>
    {
        if(field.Equals(value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    private void EditorConfigurePropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (!UseDefaultConfigure)
            return;

        switch (eventArgs.PropertyName)
        {
            case nameof(IEditorConfigure.BorderColor):
                SetBorderColor(editorConfigure.BorderColor);
                break;
            case nameof(IEditorConfigure.FillColor):
                SetFillColor(editorConfigure.FillColor);
                break;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        editorConfigure.PropertyChanged -= EditorConfigurePropertyChanged;
    }
}
