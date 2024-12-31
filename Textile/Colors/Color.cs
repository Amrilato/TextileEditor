namespace Textile.Colors;

/// <summary>
/// Represents a color value in ARGB format.
/// </summary>
public struct Color
{
    private uint Value;

    /// <summary>
    /// Gets or sets the alpha component of the color.
    /// </summary>
    public byte Alpha
    {
        readonly get => (byte)((Value & 0xFF000000) >> 24);
        set => Value = Value & 0x00FFFFFF | (uint)value << 24;
    }

    /// <summary>
    /// Gets or sets the red component of the color.
    /// </summary>
    public byte Red
    {
        readonly get => (byte)((Value & 0x00FF0000) >> 16);
        set => Value = Value & 0xFF00FFFF | (uint)value << 16;
    }

    /// <summary>
    /// Gets or sets the green component of the color.
    /// </summary>
    public byte Green
    {
        readonly get => (byte)((Value & 0x0000FF00) >> 8);
        set => Value = Value & 0xFFFF00FF | (uint)value << 8;
    }

    /// <summary>
    /// Gets or sets the blue component of the color.
    /// </summary>
    public byte Blue
    {
        readonly get => (byte)(Value & 0x000000FF);
        set => Value = Value & 0xFFFFFF00 | value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct with specified alpha, red, green, and blue components.
    /// </summary>
    /// <param name="alpha">The alpha component of the color.</param>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    public Color(byte alpha, byte red, byte green, byte blue) => Value = (uint)(alpha << 24 | red << 16 | green << 8 | blue);

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct with a specified 32-bit ARGB value.
    /// </summary>
    /// <param name="value">The 32-bit ARGB value of the color.</param>
    public Color(uint value) => Value = value;

    /// <summary>
    /// Determines whether two <see cref="Color"/> instances are equal.
    /// </summary>
    /// <param name="lhs">The first color to compare.</param>
    /// <param name="rhs">The second color to compare.</param>
    /// <returns><c>true</c> if the colors are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Color lhs, Color rhs) => lhs.Value == rhs.Value;

    /// <summary>
    /// Determines whether two <see cref="Color"/> instances are not equal.
    /// </summary>
    /// <param name="lhs">The first color to compare.</param>
    /// <param name="rhs">The second color to compare.</param>
    /// <returns><c>true</c> if the colors are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Color lhs, Color rhs) => lhs.Value != rhs.Value;

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Color"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current color.</param>
    /// <returns><c>true</c> if the specified object is equal to the current color; otherwise, <c>false</c>.</returns>
    public override readonly bool Equals(object? obj) => obj is Color color && Value == color.Value;

    /// <summary>
    /// Returns the hash code for this <see cref="Color"/> instance.
    /// </summary>
    /// <returns>A hash code for the current color.</returns>
    public override readonly int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Returns a string representation of the <see cref="Color"/> in ARGB format.
    /// </summary>
    /// <returns>A string representing the color in ARGB format.</returns>
    public override readonly string ToString() => $"ARGB({Alpha}, {Red}, {Green}, {Blue})";
}
