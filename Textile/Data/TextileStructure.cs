using DotNext.Buffers;
using System.Buffers;
using System.Runtime.InteropServices;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using static Textile.Data.TextileBase;

namespace Textile.Data;


public interface ITextileStructureSize
{
    int TieupWidth { get; }
    int TieupHeight { get; }
    int TextileWidth { get; }
    int TextileHeight { get; }
}
public readonly record struct TextileStructureSize(int TieupWidth, int TieupHeight, int TextileWidth, int TextileHeight) : ITextileStructureSize;

public class TextileStructure : IReadOnlyTextileStructure
{
    public Heddle Heddle { get; }
    public Pedal Pedal { get; }
    public Tieup Tieup { get; }
    public Textile Textile { get; }
    public HeddleColor HeddleColor { get; }
    public PedalColor PedalColor { get; }

    IReadOnlyObservableTextile<TextileIndex, bool> IReadOnlyTextileStructure.Heddle => Heddle;
    IReadOnlyObservableTextile<TextileIndex, bool> IReadOnlyTextileStructure.Pedal => Pedal;
    IReadOnlyObservableTextile<TextileIndex, bool> IReadOnlyTextileStructure.Tieup => Tieup;
    IReadOnlyObservableTextile<TextileIndex, bool> IReadOnlyTextileStructure.Textile => Textile;
    IReadOnlyObservableTextile<int, Color> IReadOnlyTextileStructure.HeddleColor => HeddleColor;
    IReadOnlyObservableTextile<int, Color> IReadOnlyTextileStructure.PedalColor => PedalColor;

    public TextileStructure(ITextileStructureSize size) : this(size.TieupWidth, size.TieupHeight, size.TextileWidth, size.TextileHeight) { }
    public TextileStructure(Heddle heddle, Pedal pedal, Tieup tieup, Textile textile, HeddleColor heddleColor, PedalColor pedalColor)
    {
        Heddle = heddle ?? throw new ArgumentNullException(nameof(heddle));
        Pedal = pedal ?? throw new ArgumentNullException(nameof(pedal));
        Tieup = tieup ?? throw new ArgumentNullException(nameof(tieup));
        Textile = textile ?? throw new ArgumentNullException(nameof(textile));
        HeddleColor = heddleColor ?? throw new ArgumentNullException(nameof(heddleColor));
        PedalColor = pedalColor ?? throw new ArgumentNullException(nameof(pedalColor));
    }

    public TextileStructure(int tieupWidth, int tieupHeight, int textileWidth, int textileHeight)
    {
        Heddle = new(textileWidth, tieupHeight);
        Pedal = new(tieupWidth, textileHeight);
        Tieup = new(tieupWidth, tieupHeight);
        Textile = new(textileWidth, textileHeight);
        HeddleColor = new(textileWidth);
        PedalColor = new(textileHeight);
    }

    public TextileStructure(Textile textile, Heddle heddle, Pedal pedal, Tieup tieup, HeddleColor heddleColor, PedalColor pedalColor)
    {
        Textile = textile ?? throw new ArgumentNullException(nameof(textile));
        Heddle = heddle ?? throw new ArgumentNullException(nameof(heddle));
        Pedal = pedal ?? throw new ArgumentNullException(nameof(pedal));
        Tieup = tieup ?? throw new ArgumentNullException(nameof(tieup));
        HeddleColor = heddleColor ?? throw new ArgumentNullException(nameof(heddleColor));
        PedalColor = pedalColor ?? throw new ArgumentNullException(nameof(pedalColor));
    }

    public void BuildTextileToOther()
    {
        var allocator = ArrayPool<int>.Shared.ToAllocator();
        using BufferWriterSlim<int> verticalBuffer = new(stackalloc int[Textile.Width * 2], allocator);
        using BufferWriterSlim<int> horizontalBuffer = new(stackalloc int[Textile.Height * 2], allocator);
        var verticalTextilePatterns = PatternList<VerticalLines>.CollectPatterns(new(Textile), verticalBuffer);
        if (verticalTextilePatterns.IdByMinLineIndex.Length > Heddle.Height)
            throw new InvalidOperationException($"overflow heddle");

        var horizontalTextilePatterns = PatternList<HorizontalLines>.CollectPatterns(new(Textile), horizontalBuffer);
        if (horizontalTextilePatterns.IdByMinLineIndex.Length > Pedal.Width)
            throw new InvalidOperationException($"overflow pedal");

        Heddle.PatternIdByIndexAtArrayConsecutiveLine(verticalTextilePatterns);
        Pedal.PatternIdByIndexAtArrayConsecutiveLine(horizontalTextilePatterns);

        using PoolingArrayTextile tieup = new(Tieup.Width, Tieup.Height);
        for (int x = 0; x < Math.Min(tieup.Width, horizontalTextilePatterns.IdByMinLineIndex.Length); x++)
        {
            for (int y = 0; y < Math.Min(tieup.Height, verticalTextilePatterns.IdByMinLineIndex.Length); y++)
            {
                tieup[new(x, y)] = verticalTextilePatterns.lines[verticalTextilePatterns.IdByMinLineIndex[y]].Span.IsBitSet(horizontalTextilePatterns.IdByMinLineIndex[x]);
            }
        }

        Tieup.CopyFrom(tieup);
    }
    public void BuildOtherToTextile()
    {
        using PoolingArrayTextile textile = new(Textile.Width, Textile.Height);
        Span<uint> buffer = stackalloc uint[Heddle.Height.GetArraySize()];

        for (int pedalY = 0; pedalY < Pedal.Height; pedalY++)
        {
            var current = Pedal.HorizontalLine(pedalY);
            for (int pedalX = 0; pedalX < Pedal.Width; pedalX++)
                if (current.Span.IsBitSet(pedalX))
                    Tieup.VerticalLine(pedalX).Span.XOR(buffer);
            for (int heddleX = 0; heddleX < Heddle.Width; heddleX++)
                if (AndAny(buffer, Heddle.VerticalLine(heddleX).Span))
                    textile[new(heddleX, pedalY)] = true;
            buffer.Clear();
        }

        Textile.CopyFrom(textile);

        static bool AndAny(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(a.Length, b.Length);
            Span<uint> buffer = stackalloc uint[a.Length];
            a.CopyTo(buffer);
            b.And(buffer);
            return buffer.ContainsAnyExcept(0u);
        }
    }
    public void Serialize(IBufferWriter<byte> bufferWriter)
    {
        Serialize(bufferWriter, Textile);
        Serialize(bufferWriter, Heddle);
        Serialize(bufferWriter, Pedal);
        Serialize(bufferWriter, Tieup);
        Serialize(bufferWriter, HeddleColor);
        Serialize(bufferWriter, PedalColor);
    }

    private static void Serialize(IBufferWriter<byte> bufferWriter, TextileBase textileData)
    {
        var span = textileData.AsSpan();
        var buffer = MemoryMarshal.Cast<byte, uint>(bufferWriter.GetSpan(span.Length * 4 + 12));
        buffer[0] = (uint)span.Length;
        buffer[1] = (uint)textileData.Width;
        buffer[2] = (uint)textileData.Height;
        span.CopyTo(buffer[3..]);
        bufferWriter.Advance((span.Length + 3) * 4);
    }
    private static void Serialize(IBufferWriter<byte> bufferWriter, TextileColor textileColor)
    {
        var span = MemoryMarshal.Cast<Color, int>(textileColor.AsSpan());
        var buffer = MemoryMarshal.Cast<byte, int>(bufferWriter.GetSpan(span.Length * 4 + 4));
        buffer[0] = span.Length;
        span.CopyTo(buffer[1..]);
        bufferWriter.Advance((span.Length + 1) * 4);
    }

    public static TextileStructure Deserialize(ReadOnlySpan<byte> buffer) => Deserialize(MemoryMarshal.Cast<byte, uint>(buffer));
    public static TextileStructure Deserialize(ReadOnlySpan<uint> buffer)
    {
        int next = Deserialize(buffer, out Textile textile);
        next += Deserialize(buffer[next..], out Heddle heddle);
        next += Deserialize(buffer[next..], out Pedal pedal);
        next += Deserialize(buffer[next..], out Tieup tieup);
        next += Deserialize(buffer[next..], out HeddleColor heddleColor);
        Deserialize(buffer[next..], out PedalColor pedalColor);
        return new(textile, heddle, pedal, tieup, heddleColor, pedalColor);
    }
    private static int Deserialize<T>(ReadOnlySpan<uint> buffer, out T result)
        where T : TextileBase, ICreateTextile<T>
    {
        var length = (int)buffer[0];
        result = T.Create((int)buffer[1], (int)buffer[2]);
        buffer[3..(length + 3)].CopyTo(result.AsSpan());
        return length + 3;
    }
    private static int Deserialize<T>(ReadOnlySpan<uint> buffer, out T result, int _ = 0)
        where T : TextileColor, ICreateTextileColor<T>
    {
        var length = (int)buffer[0];
        result = T.Create(length);
        buffer[1..(length + 1)].CopyTo(MemoryMarshal.Cast<Color, uint>(result.AsSpan()));
        return length + 1;
    }

    public TextileStructure Resize(int tieupWidth, int tieupHeight, int textileWidth, int textileHeight)
    {
        TextileStructure structure = new(tieupWidth, tieupHeight, textileWidth, textileHeight);
        structure.Heddle.CopyFrom(Heddle);
        structure.Pedal.CopyFrom(Pedal);
        structure.Tieup.CopyFrom(Tieup);
        structure.Textile.CopyFrom(Textile);
        structure.HeddleColor.CopyFrom(HeddleColor);
        structure.PedalColor.CopyFrom(PedalColor);

        return structure;
    }
}
