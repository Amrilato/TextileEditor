using System.Collections.Immutable;
using Textile.Colors;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;
using TextileEditor.Shared.Serialization.Textile;

namespace TextileEditor.Shared.Common.Logger;

public class TextileLogger : IDisposable
{
    private readonly TextileData textileData;
    private bool SuppressLogging = false;
    private readonly BoundedStack<ITextileLog> undo;
    private readonly BoundedStack<ITextileLog> redo;

    public TextileLogger(TextileData textileData, int capacity = 30)
    {
        this.textileData = textileData;
        Subscribe(textileData.TextileStructure);
        undo = new(capacity);
        redo = new(capacity);
    }

    private void Subscribe(TextileStructure TextileStructure)
    {
        TextileStructure.Textile.TextileStateChanged += TextileStateChanged;
        TextileStructure.Heddle.TextileStateChanged += TextileStateChanged;
        TextileStructure.Pedal.TextileStateChanged += TextileStateChanged;
        TextileStructure.Tieup.TextileStateChanged += TextileStateChanged;
        TextileStructure.HeddleColor.TextileStateChanged += TextileStateChanged;
        TextileStructure.PedalColor.TextileStateChanged += TextileStateChanged;
    }
    private void Unsubscribe(TextileStructure TextileStructure)
    {
        TextileStructure.Textile.TextileStateChanged -= TextileStateChanged;
        TextileStructure.Heddle.TextileStateChanged -= TextileStateChanged;
        TextileStructure.Pedal.TextileStateChanged -= TextileStateChanged;
        TextileStructure.Tieup.TextileStateChanged -= TextileStateChanged;
        TextileStructure.HeddleColor.TextileStateChanged -= TextileStateChanged;
        TextileStructure.PedalColor.TextileStateChanged -= TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<int, Color> sender, TextileStateChangedEventArgs<int, Color> eventArgs)
    {
        if (SuppressLogging || sender is not ITextile<int, Color> textile)
            return;
        undo.Push(new TextileDataChangedLog<int, Color>(textile, eventArgs.ChangedIndices.ToImmutableArray()));
        redo.Clear();
        InvokeLoggerStateChanged();
    }
    private void TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs)
    {
        if (SuppressLogging || sender is not ITextile<TextileIndex, bool> textile)
            return;
        undo.Push(new TextileDataChangedLog<TextileIndex, bool>(textile, eventArgs.ChangedIndices.ToImmutableArray()));
        redo.Clear();
        InvokeLoggerStateChanged();
    }

    private void InvokeLoggerStateChanged() => LoggerStateChanged?.Invoke();
    public event Action? LoggerStateChanged;


    public bool CanUndo() => undo.Count > 0;
    public bool CanRedo() => redo.Count > 0;

    public bool TryUndo()
    {
        if (undo.TryPop(out var undoItem))
        {
            try
            {
                SuppressLogging = true;
                undoItem.Undo();
                redo.Push(undoItem);
                SuppressLogging = false;
                InvokeLoggerStateChanged();
                return true;
            }
            catch (Exception)
            {
                SuppressLogging = false;
                undo.Clear();
                redo.Clear();
                return false;
            }
        }
        else
            return false;
    }
    public bool TryRedo()
    {
        if (redo.TryPop(out var redoItem))
        {
            try
            {
                SuppressLogging = true;
                redoItem.Redo();
                undo.Push(redoItem);
                SuppressLogging = false;
                InvokeLoggerStateChanged();
                return true;
            }
            catch (Exception)
            {
                SuppressLogging = false;
                undo.Clear();
                redo.Clear();
                return false;
            }
        }
        else
            return false;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Unsubscribe(textileData.TextileStructure);
    }
}

file class TextileDataChangedLog<TIndex, TValue>(ITextile<TIndex, TValue> textile, ImmutableArray<ChangedValue<TIndex, TValue>> changedValues) : ITextileLog
{
    public void Redo()
    {
        textile.Write(changedValues.Select(v => new KeyValuePair<TIndex, TValue>(v.Index, v.Current)));
    }

    public void Undo()
    {
        textile.Write(changedValues.Select(v => new KeyValuePair<TIndex, TValue>(v.Index, v.Previous)));
    }
}