using R3;
using SkiaSharp;

namespace TextileEditor.Shared.View.Common;

/// <summary>
/// Represents a painter interface defining the required properties and methods for painting.
/// </summary>
public interface IPainter
{
    /// <summary>
    /// Attempts to paint the surface with the provided information.
    /// </summary>
    /// <param name="surface">The surface to paint on.</param>
    /// <param name="info">The image info for the surface.</param>
    /// <param name="rawInfo">The raw image info for the surface.</param>
    /// <returns>True if painting succeeded, otherwise false.</returns>
    bool TryPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo);

    /// <summary>
    /// Gets the current render progress as a read-only reactive property.
    /// </summary>
    ReadOnlyReactiveProperty<RenderProgress> RenderingProgress { get; }

    /// <summary>
    /// Gets the size of the canvas.
    /// </summary>
    SKSizeI CanvasSize { get; }
}
