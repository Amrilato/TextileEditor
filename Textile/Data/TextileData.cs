using DotNext.Buffers;
using System.Buffers;
using Textile.Common;
using Textile.Interfaces;

namespace Textile.Data;

public abstract class TextileData(int width, int height, uint[] value) : IObservableTextile<TextileIndex, bool>, IReadOnlyObservableTextile<TextileIndex, bool>
{
    public int Width { get; } = width;
    public int Height { get; } = height;
    private int version = 0;
    private readonly uint[] Value = value;
    internal Span<uint> AsSpan() => Value;

    private TextileIndex[]? CacheIndices;


    public bool this[TextileIndex index]
    {
        get
        {
            this.ThrowIfIndexWasOutOfBound(index);
            return ArrayConsecutiveLine(index).Span.IsBitSet(ArrayConsecutiveIndex(index));
        }
        set
        {
            this.ThrowIfIndexWasOutOfBound(index);
            var previous = new ChangedValue<TextileIndex, bool>(index, value, this[index]);
            SetValue(index, value);
            StateHasChanged(new(ref previous));
        }
    }

    protected virtual void SetValue(TextileIndex index, bool value)
    {
        ArrayConsecutiveLine(index).Span.SetBit(ArrayConsecutiveIndex(index), value);
        version++;
    }

    public void CopyFrom<T>(T source, TextileIndex destinationOffset = default, TextileIndex sourceOffset = default)
        where T : IReadOnlyTextile<TextileIndex, bool>
    {
        int width = Math.Min(source.Width - sourceOffset.X, Width - destinationOffset.X);
        int height = Math.Min(source.Height - sourceOffset.Y, Height - destinationOffset.Y);

        Span<ChangedValue<TextileIndex, bool>> changedIndices = stackalloc ChangedValue<TextileIndex, bool>[width * height];
        int changedIndex = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TextileIndex readIndex = sourceOffset + new TextileIndex(x, y);
                TextileIndex writeIndex = destinationOffset + new TextileIndex(x, y);
                if (this[writeIndex] != source[readIndex])
                {
                    changedIndices[changedIndex++] = new(writeIndex, source[readIndex], this[writeIndex]);
                    SetValue(writeIndex, source[readIndex]);
                }
            }
        }
        StateHasChanged(changedIndices[..changedIndex]);
    }

    public TextileData Clip(TextileRange range)
    {
        ClipTextile clip = new(range.Width, range.Height);
        clip.CopyFrom(this, sourceOffset: new(range.Left, range.Top));
        return clip;
    }

    public void Write(IEnumerable<KeyValuePair<TextileIndex, bool>> values)//todo: check duplicate index
    {
        int index = 0;
        Span<ChangedValue<TextileIndex, bool>> changedValues = stackalloc ChangedValue<TextileIndex, bool>[Width * Height];
        foreach (var (i, value) in values)
        {
            if (this[i] == value)
                continue;
            changedValues[index++] = new(i, value, !value);
            SetValue(i, value);
        }
        StateHasChanged(changedValues[..index]);
    }

    public void Clear()
    {
        int index = 0;
        Span<ChangedValue<TextileIndex, bool>> changedValues = stackalloc ChangedValue<TextileIndex, bool>[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (this[new(x,y)])
                {
                    changedValues[index++] = new(new(x, y), false, true);
                    SetValue(new(x, y), false);
                }
            }
        }
        StateHasChanged(changedValues[..index]);
    }

    public event TextileStateChangedEventHandler<TextileIndex, bool>? TextileStateChanged;
    protected void StateHasChanged(ReadOnlySpan<ChangedValue<TextileIndex, bool>> changedIndices) => TextileStateChanged?.Invoke(this, new(changedIndices));

    protected abstract int ArrayConsecutiveLength { get; }
    protected abstract int ArrayNonConsecutiveLength { get; }
    protected abstract int ArrayConsecutiveIndex(TextileIndex index);
    protected abstract int ArrayNonConsecutiveIndex(TextileIndex index);
    protected abstract TextileIndex ToIndex(int ConsecutiveIndex, int NonConsecutiveIndex);

    protected Memory<uint> ArrayConsecutiveLine(TextileIndex index) => ArrayConsecutiveLine(ArrayNonConsecutiveIndex(index));
    protected Memory<uint> ArrayConsecutiveLine(int index)
    {
        var size = ArrayConsecutiveLength.GetArraySize();
        return Value.AsMemory()[(index * size)..(index * size + size)];
    }

    protected ReadOnlySpan<uint> ArrayNonConsecutiveLine(TextileIndex index, Span<uint> buffer) => ArrayNonConsecutiveLine(ArrayConsecutiveIndex(index), buffer);
    protected ReadOnlySpan<uint> ArrayNonConsecutiveLine(int index, Span<uint> buffer)
    {
        if ((uint)buffer.Length < (uint)ArrayNonConsecutiveLength.GetArraySize() || (uint)ArrayConsecutiveLength < (uint)index)
            throw new ArgumentOutOfRangeException(nameof(buffer), "Invalid buffer length or index.");
        ref uint value = ref buffer[0];

        for (int i = 0; i < ArrayNonConsecutiveLength; i++)
        {
            int current = i % 32;
            if (current == 0)
                value = ref buffer[i.GetArrayOffset()];

            if (ArrayConsecutiveLine(i).Span.IsBitSet(index))
                value |= BitManipulation.GetSingleBitSet(current);
        }
        return buffer;
    }

    internal abstract ReadOnlyMemory<uint> HorizontalLine(int index);
    internal abstract ReadOnlyMemory<uint> VerticalLine(int index);

    internal PatternList<HorizontalLines> CollectHorizontalLinePatterns(BufferWriterSlim<int> bufferProvider) => PatternList<HorizontalLines>.CollectPatterns(new(this), bufferProvider);
    internal PatternList<VerticalLines> CollectVerticalLinePatterns(BufferWriterSlim<int> bufferProvider) => PatternList<VerticalLines>.CollectPatterns(new(this), bufferProvider);

    #region PatternList
    internal interface ILines
    {
        int Version { get; }
        ReadOnlyMemory<uint> this[int index] { get; }
        int MemoryLength { get; }
        int Count { get; }
    }

    internal readonly struct HorizontalLines(TextileData textile) : ILines
    {
        private readonly TextileData textile = textile;
        public ReadOnlyMemory<uint> this[int index] => textile.HorizontalLine(index);
        public int Version => textile.version;
        public int MemoryLength => textile.Height;
        public int Count => textile.Width;
    }
    internal readonly struct VerticalLines(TextileData textile) : ILines
    {
        private readonly TextileData textile = textile;
        public ReadOnlyMemory<uint> this[int index] => textile.VerticalLine(index);
        public int Version => textile.version;
        public int MemoryLength => textile.Width;
        public int Count => textile.Height;
    }

    internal readonly ref struct PatternList<TLine>
        where TLine : ILines
    {
        private PatternList(ReadOnlySpan<int> idByMinLineIndex, ReadOnlySpan<int> lineByPatternId, TLine lines)
        {
            IdByMinLineIndex = idByMinLineIndex;
            LineByPatternId = lineByPatternId;
            this.lines = lines;
            version = lines.Version;
        }

        public readonly int version;
        public readonly ReadOnlySpan<int> IdByMinLineIndex;
        public readonly ReadOnlySpan<int> LineByPatternId;
        public readonly TLine lines;

        public static PatternList<TLine> CollectPatterns(TLine lines, BufferWriterSlim<int> bufferProvider)
        {
            Span<int> buffer = bufferProvider.GetSpan(lines.MemoryLength * 2);
            Span<int> lineByPatternId = buffer[..lines.MemoryLength];
            Span<int> idByMinLineIndex = buffer[lines.MemoryLength..];
            int patternCount = 0;
            for (int i = 0; i < lines.MemoryLength; i++)
            {
                if (!lines[i].Span.ContainsAnyExcept(0u))
                {
                    lineByPatternId[i] = -1;
                    continue;
                }

                var patternId = IsExistsPattern(lines[i].Span, idByMinLineIndex[..patternCount], lines);
                if (patternId > -1)
                {
                    lineByPatternId[i] = patternId;
                }
                else
                {
                    idByMinLineIndex[patternCount] = i;
                    lineByPatternId[i] = patternCount;
                    patternCount++;
                }
            }

            return new(idByMinLineIndex[..patternCount], lineByPatternId, lines);

            static int IsExistsPattern(ReadOnlySpan<uint> line, ReadOnlySpan<int> indexes, TLine lines)
            {
                for (int i = 0; i < indexes.Length; i++)
                {
                    if (lines[indexes[i]].Span.SequenceEqual(line))
                        return i;
                }
                return -1;
            }
        }
    }
    #endregion

    private void ClearArrayConsecutiveLine(int index, ref BufferWriterSlim<ChangedValue<TextileIndex, bool>> changedIndicesBuffer)
    {
        var changedIndices = changedIndicesBuffer.GetSpan(ArrayNonConsecutiveLength);
        int changedIndexCount = 0;
        var line = ArrayConsecutiveLine(index);
        for (int i = 0; i < line.Length; i++)
        {
            if (line.Span[i] == 0)
                continue;

            for (int x = 0; x < BitManipulation.BitLength; x++)
            {
                if (line.Span[i].IsBitSet(x))
                    changedIndices[changedIndexCount++] = new(ToIndex(index, i * BitManipulation.BitLength + x), false, true);
            }
        }
        changedIndicesBuffer.Advance(changedIndexCount);
        line.Span.Clear();
    }
    private void OverwriteArrayConsecutiveLine(int index, ref BufferWriterSlim<ChangedValue<TextileIndex, bool>> changedIndicesBuffer, ReadOnlySpan<uint> source)
    {
        var destination = ArrayConsecutiveLine(index);

        var changedIndices = changedIndicesBuffer.GetSpan(ArrayNonConsecutiveLength);
        int changedIndexCount = 0;
        for (int i = 0; i < destination.Length; i++)
        {
            if (destination.Span[i] == source[i])
                continue;

            for (int bitIndex = 0; bitIndex < BitManipulation.BitLength; bitIndex++)
            {
                var sourceValue = source[i].IsBitSet(bitIndex);
                var destinationValue = destination.Span[i].IsBitSet(bitIndex);
                if (destinationValue != sourceValue)
                {
                    changedIndices[changedIndexCount++] = new(ToIndex(i * BitManipulation.BitLength + bitIndex, index), sourceValue, destinationValue);
                    BitManipulation.SetBit(ref destination.Span[i], bitIndex, sourceValue);
                }
            }
        }
        changedIndicesBuffer.Advance(changedIndexCount);
    }

    internal void PatternIdByIndexAtArrayConsecutiveLine<TLine>(PatternList<TLine> patternList)
        where TLine : ILines
    {
        if (patternList.version < patternList.lines.Version)
            throw new ArgumentOutOfRangeException(nameof(patternList), "Invalid pattern list version. The provided version is too low.");
        if (ArrayNonConsecutiveLength != patternList.lines.MemoryLength)
            throw new ArgumentOutOfRangeException(nameof(patternList), "Invalid pattern list length. The memory length does not match the expected value.");


        BufferWriterSlim<ChangedValue<TextileIndex, bool>> changedIndices = new(stackalloc ChangedValue<TextileIndex, bool>[Width * Height], ArrayPool<ChangedValue<TextileIndex, bool>>.Shared.ToAllocator());
        try
        {
            Span<uint> buffer = stackalloc uint[ArrayConsecutiveLength.GetArraySize()];


            for (int i = 0; i < Math.Min(ArrayNonConsecutiveLength, patternList.lines.Count); i++)
            {
                if (patternList.LineByPatternId[i] > -1)
                {
                    buffer.Clear();
                    buffer.SetBit(patternList.LineByPatternId[i], true);
                    OverwriteArrayConsecutiveLine(i, ref changedIndices, buffer);
                }
                else
                    ClearArrayConsecutiveLine(i, ref changedIndices);
            }
            if (ArrayNonConsecutiveLength > patternList.lines.Count)
                for (int i = patternList.lines.Count; i < ArrayNonConsecutiveLength; i++)
                    ClearArrayConsecutiveLine(i, ref changedIndices);

            StateHasChanged(changedIndices.WrittenSpan);
        }
        finally
        {
            changedIndices.Dispose();
        }
    }
    public IEnumerable<TextileIndex> Indices
    {
        get
        {
            return CacheIndices ??= Create().ToArray();

            IEnumerable<TextileIndex> Create()
            {
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        yield return new(x, y);
            }
        }
    }
}
