namespace Textile.Common;

public static class BitManipulation
{
    public const int BitLength = 32;
    /// <summary>
    /// Calculates the size of an array based on the specified length, considering a fixed chunk size of 32.
    /// </summary>
    /// <param name="length">The length used to determine the array size.</param>
    /// <returns>The size of the array based on the specified length.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the length is a negative number.</exception>
    public static int GetArraySize(this int length)
    {
        if (length < 0)
            ThrowArgumentOutOfRange(nameof(length), "Length must be a non-negative number.");

        return (int)Math.Ceiling(length / 32d);
    }

    /// <summary>
    /// Calculates the value of an array based on the specified length, considering a fixed chunk size of 32.
    /// </summary>
    /// <param name="index">The index used to determine the array offset.</param>
    /// <returns>The offset of the array based on the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is a negative number.</exception>
    public static int GetArrayOffset(this int index)
    {
        if (index < 0)
            ThrowArgumentOutOfRange(nameof(index), "Index must be a non-negative number.");

        return index / 32;
    }
    /// <summary>
    /// Determines whether the bit at the specified index is set in the given unsigned 32-bit integer.
    /// </summary>
    /// <param name="bits">The unsigned 32-bit integer containing the bits.</param>
    /// <param name="index">The index of the bit to check.</param>
    /// <returns>
    /// <c>true</c> if the bit at the specified index is set; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified index is outside the valid range (0 to 31, inclusive).
    /// </exception>
    public static bool IsBitSet(this uint bits, int index)
    {
        if ((uint)index > 31u)
            ThrowArgumentOutOfRange(nameof(index), "The specified index is outside the valid range (0 to 31, inclusive).");
        return (bits & 1u << index) > 0u;
    }
    /// <summary>
    /// Determines whether the bit at the specified index is set in the given span of unsigned 32-bit integers.
    /// </summary>
    /// <param name="bits">The span of unsigned 32-bit integers containing the bits.</param>
    /// <param name="index">The index of the bit to check.</param>
    /// <returns>
    /// <c>true</c> if the bit at the specified index is set; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified index is outside the valid range for the provided span.
    /// </exception>
    public static bool IsBitSet(this Span<uint> bits, int index)
    {
        var offset = index.GetArrayOffset();
        if ((uint)bits.Length <= (uint)offset)
            ThrowArgumentOutOfRange(nameof(bits), "The specified index is outside the valid range for the provided span.");
        return bits[offset].IsBitSet(index - offset * 32);
    }
    /// <summary>
    /// Determines whether the bit at the specified index is set in the given read-only span of unsigned 32-bit integers.
    /// </summary>
    /// <param name="bits">The read-only span of unsigned 32-bit integers containing the bits.</param>
    /// <param name="index">The index of the bit to check.</param>
    /// <returns>
    /// <c>true</c> if the bit at the specified index is set; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified index is outside the valid range for the provided read-only span.
    /// </exception>
    public static bool IsBitSet(this ReadOnlySpan<uint> bits, int index)
    {
        var offset = index.GetArrayOffset();
        if ((uint)bits.Length <= (uint)offset)
            ThrowArgumentOutOfRange(nameof(bits), "The specified index is outside the valid range for the provided read-only span.");
        return bits[offset].IsBitSet(index - offset * 32);
    }
    /// <summary>
    /// Determines whether the bit at the specified index is set in the given array of unsigned 32-bit integers.
    /// </summary>
    /// <param name="bits">The array of unsigned 32-bit integers containing the bits.</param>
    /// <param name="index">The index of the bit to check.</param>
    /// <returns>
    /// <c>true</c> if the bit at the specified index is set; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified index is outside the valid range for the provided array.
    /// </exception>
    public static bool IsBitSet(this uint[] bits, int index)
    {
        var offset = index.GetArrayOffset();
        if ((uint)bits.Length <= (uint)offset)
            ThrowArgumentOutOfRange(nameof(bits), "The specified index is outside the valid range for the provided array.");
        return bits[offset].IsBitSet(index - offset * 32);
    }
    /// <summary>
    /// Sets the bit at the specified index in the given 32-bit unsigned integer.
    /// </summary>
    /// <param name="bits">The 32-bit unsigned integer to modify.</param>
    /// <param name="index">The index of the bit to set (0 to 31, inclusive).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified index is outside the valid range (0 to 31, inclusive).
    /// </exception>
    public static void SetBit(ref uint bits, int index, bool value)
    {
        if (value)
            bits |= GetSingleBitSet(index);
        else
            bits &= ~GetSingleBitSet(index);
    }

    /// <summary>
    /// Sets the bit at the specified index in the given array of 32-bit unsigned integers.
    /// </summary>
    /// <param name="bits">The array of 32-bit unsigned integers to modify.</param>
    /// <param name="index">The index of the bit to set (0 to (bits.MemoryLength * 32) - 1, inclusive).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified index is negative or greater than or equal to (bits.MemoryLength * 32).
    /// </exception>
    // <remarks>
    // If the specified index is outside the range of the array, this method does nothing.
    // </remarks>
    public static void SetBit(this Span<uint> bits, int index, bool value)
    {
        if ((uint)index >= (uint)bits.Length * 32u)
            ThrowArgumentOutOfRange(nameof(index), $"Index must be between 0 and {bits.Length * 32 - 1} (inclusive).");

        var offset = index.GetArrayOffset();

        // Set the bit in the appropriate 32-bit unsigned integer within the array
        SetBit(ref bits[offset], index - offset * 32, value);
    }
    /// <summary>
    /// Retrieves a 32-bit unsigned integer with a single bit set at the specified position.
    /// </summary>
    /// <param name="n">The position of the bit to set (0 to 31, inclusive).</param>
    /// <returns>
    /// A 32-bit unsigned integer with only the bit at the specified position set.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified position is less than 0 or greater than or equal to 32.
    /// </exception>
    public static uint GetSingleBitSet(int n)
    {
        if ((uint)n >= 32u)
            ThrowArgumentOutOfRange(nameof(n), "n must be between 0 and 31 (inclusive).");

        return (uint)(1 << n);
    }
    /// <summary>
    /// Checks if any bit set is present in the given array of unsigned integers.
    /// </summary>
    /// <param name="bits">An array of unsigned integers representing a set of bits.</param>
    /// <returns>
    ///   <c>true</c> if any bit set is found in the array; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///   This method iterates through the array and returns true if any element has a non-zero value,
    ///   indicating the presence of at least one set bit.
    /// </remarks>
    public static bool HasAnySet(this uint[] bits) => bits.AsSpan().ContainsAnyExcept(0u);
    /// <summary>
    /// Checks if any bit set is present in the given span of unsigned integers.
    /// </summary>
    /// <param name="bits">An span of unsigned integers representing a set of bits.</param>
    /// <returns>
    ///   <c>true</c> if any bit set is found in the array; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///   This method iterates through the array and returns true if any element has a non-zero value,
    ///   indicating the presence of at least one set bit.
    /// </remarks>
    public static bool HasAnySet(this Span<uint> bits) => bits.ContainsAnyExcept(0u);

    /// <summary>
    /// Inverts the orientation of a 2D bit array, copying the bits from the specified X index
    /// to a one-dimensional span of 32-bit unsigned integers.
    /// </summary>
    /// <param name="bits">The 2D array of bits to invert.</param>
    /// <param name="buffer">The destination span where the inverted bits will be written.</param>
    /// <param name="index">The X index of the bits to be copied.</param>
    /// <remarks>
    /// This method transposes the 2D bit array such that the specified X index becomes the new Y index.
    /// The inverted bits are then written to the provided span, packed into 32-bit unsigned integers.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the length of the buffer is insufficient or if the specified index is out of bounds.
    /// </exception>
    public static void InvertOrientation(this uint[][] bits, Span<uint> buffer, int index)
    {
        if ((uint)buffer.Length < (uint)bits.Length.GetArraySize() || (uint)bits.Length < (uint)index)
            ThrowArgumentOutOfRange(nameof(buffer), "Invalid buffer length or index.");

        ref uint value = ref buffer[0];

        for (int i = 0; i < bits.Length; i++)
        {
            int current = i % 32;
            if (current == 0)
                value = ref buffer[i.GetArrayOffset()];

            if (bits[i].IsBitSet(index))
                value |= GetSingleBitSet(current);
        }
    }
    public static void InvertOrientation(this ReadOnlySpan<uint> bits, int height, Span<uint> buffer, int index)
    {
        if ((uint)buffer.Length < (uint)bits.Length.GetArraySize() || (uint)bits.Length < (uint)index)
            ThrowArgumentOutOfRange(nameof(buffer), "Invalid buffer length or index.");

        ref uint value = ref buffer[0];
        var aLength = height.GetArraySize();
        var length = bits.Length / aLength;
        var bitsIndex = 0;

        for (int i = 0; i < length; i++)
        {
            int current = i % 32;
            if (current == 0)
                value = ref buffer[i.GetArrayOffset()];

            if (bits[bitsIndex..(bitsIndex += aLength)].IsBitSet(index))
                value |= GetSingleBitSet(current);
        }
    }
    public static void InvertOrientation(this ReadOnlySpan<uint> bits, Span<uint> buffer, int index)
    {
        if ((uint)buffer.Length < (uint)bits.Length.GetArraySize() || (uint)bits.Length < (uint)index)
            ThrowArgumentOutOfRange(nameof(buffer), "Invalid buffer length or index.");

        ref uint value = ref buffer[0];

        for (int i = 0; i < bits.Length; i++)
        {
            int current = i % 32;
            if (current == 0)
                value = ref buffer[i.GetArrayOffset()];

            if (bits[i].IsBitSet(index))
                value |= GetSingleBitSet(current);
        }
    }
    public static void XOR(this ReadOnlySpan<uint> ros, Span<uint> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(ros.Length, buffer.Length);
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = buffer[i] ^ ros[i];
        }
    }
    public static void OR(this ReadOnlySpan<uint> ros, Span<uint> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(ros.Length, buffer.Length);
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = buffer[i] | ros[i];
        }
    }
    public static void And(this ReadOnlySpan<uint> ros, Span<uint> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(ros.Length, buffer.Length);
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = buffer[i] & ros[i];
        }
    }
    public static int LeftFilledBit(int index) => index > 0 ? -2147483648 >> index - 1 : 0;
    public static int RightFilledBit(int index) => index > 0 ? (1 << index) - 1 : 0;

    private static void ThrowArgumentOutOfRange(string name, string message) => throw new ArgumentOutOfRangeException(name, message);
}
