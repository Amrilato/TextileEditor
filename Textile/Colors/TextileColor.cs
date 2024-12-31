using Textile.Common;
using Textile.Interfaces;

namespace Textile.Colors;

public abstract class TextileColor(int length) : IObservableTextile<int, Color>, IReadOnlyObservableTextile<int, Color>, IReadOnlyTextileColor
{
    private readonly Color[] Color = new Color[length];
    internal Span<Color> AsSpan() => Color;
    public int Length => Color.Length;
    public Color this[int index]
    {
        get => Color[index];
        set
        {
            ChangedValue<int, Color> previous = new(index, value, this[index]);
            Color[index] = value;
            TextileStateChanged?.Invoke(this, new(new(ref previous)));
        }
    }

    public abstract int Width { get; }
    public abstract int Height { get; }

    public event TextileStateChangedEventHandler<int, Color>? TextileStateChanged;
    public abstract int ToIndex(TextileIndex index);
    public abstract TextileIndex ToIndex(int accessorIndex, int nonAccessorIndex);

    public void Write(IEnumerable<KeyValuePair<int, Color>> values)
    {
        int index = 0;
        Span<ChangedValue<int, Color>> changedValues = stackalloc ChangedValue<int, Color>[Width * Height];
        foreach (var (i, value) in values)
        {
            if (value == this[i])
                continue;
            changedValues[index++] = new(i, value, this[i]);
            Color[i] = value;
        }
        TextileStateChanged?.Invoke(this, new(changedValues[..index]));
    }

    public void Clear()
    {
        int index = 0;
        Color transparent = new(0, 0, 0, 0);
        Span<ChangedValue<int, Color>> changedValues = stackalloc ChangedValue<int, Color>[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var i = ToIndex(new(x, y));
                if (this[i] != transparent)
                {
                    changedValues[index++] = new(i, transparent, this[i]);
                    Color[i] = transparent;
                }
            }
        }
        TextileStateChanged?.Invoke(this, new(changedValues[..index]));
    }

    //todo: add notify color state changed
    internal void CopyFrom(TextileColor color)
    {
        var length = Math.Min(color.Color.Length, Color.Length);
        color.Color.AsSpan()[..length].CopyTo(Color.AsSpan()[..length]);
    }

    public IEnumerable<int> Indices
    {
        get
        {
            for (int i = 0; i < Color.Length; i++)
                yield return i;
        }
    }
}
