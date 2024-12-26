using SkiaSharp;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TextileEditor.Shared.View.Common;

internal interface IManagedMemorySKSurface : IDisposable
{
    /// <summary>
    /// Gets or sets the image information for the surface.
    /// </summary>
    SKImageInfo SKImageInfo { get; }

    /// <summary>
    /// Updates the current <see cref="SKImageInfo"/> and recreates the bitmap.
    /// </summary>
    /// <param name="info">The new image information.</param>
    void ChangeImageInfo(SKImageInfo info);

    /// <summary>
    /// Creates a new surface using the current <see cref="SKImageInfo"/>.
    /// </summary>
    /// <param name="info">The image information used to create the surface.</param>
    /// <returns>An <see cref="SKSurfaceOwner"/> that manages the created surface.</returns>
    ManagedMemorySKSurface.SKSurfaceOwner CreateSurface(out SKImageInfo info);

    /// <summary>
    /// Creates a new surface using the specified <see cref="SKImageInfo"/>.
    /// </summary>
    /// <param name="sourceInfo">The image information used to create the surface.</param>
    /// <returns>An <see cref="SKSurfaceOwner"/> that manages the created surface.</returns>
    ManagedMemorySKSurface.SKSurfaceOwner CreateSurface(SKImageInfo sourceInfo);

    static abstract IManagedMemorySKSurface Create();
}

/// <summary>
/// A class that manages memory for creating and managing SkiaSharp SKSurfaces.
/// Provides thread-safe operations, memory pinning, and lifecycle management for SKSurfaces.
/// </summary>
internal class ManagedMemorySKSurface : IManagedMemorySKSurface
{
    private const int SYNC_ENTER = 1;
    private const int SYNC_EXIT = 0;
    private int _syncFlag;

    /// <summary>
    /// Gets or sets the image information for the surface.
    /// </summary>
    public SKImageInfo SKImageInfo { get; private set; } = new SKImageInfo(0, 0);
    private byte[]? pixels;
    private GCHandle pixelsHandle;

    private ManagedMemorySKSurface() { }

    public static IManagedMemorySKSurface Create() => new ManagedMemorySKSurface();

    /// <summary>
    /// Creates a new surface using the current <see cref="SKImageInfo"/>.
    /// </summary>
    /// <param name="info">The image information used to create the surface.</param>
    /// <returns>An <see cref="SKSurfaceOwner"/> that manages the created surface.</returns>
    public SKSurfaceOwner CreateSurface(out SKImageInfo info)
    {
        Enter();
        CreateBitmap(SKImageInfo);
        info = SKImageInfo;
        return new(this, (SKSurface?)SKSurface.Create(SKImageInfo, pixelsHandle.AddrOfPinnedObject(), SKImageInfo.RowBytes) ?? throw new InvalidOperationException("configuration is not supported."));
    }

    /// <summary>
    /// Creates a new surface using the specified <see cref="SKImageInfo"/>.
    /// </summary>
    /// <param name="sourceInfo">The image information used to create the surface.</param>
    /// <returns>An <see cref="SKSurfaceOwner"/> that manages the created surface.</returns>
    public SKSurfaceOwner CreateSurface(SKImageInfo sourceInfo)
    {
        Enter();
        CreateBitmap(sourceInfo);
        return new(this, (SKSurface?)SKSurface.Create(SKImageInfo, pixelsHandle.AddrOfPinnedObject(), SKImageInfo.RowBytes) ?? throw new InvalidOperationException("configuration is not supported."));
    }

    /// <summary>
    /// Updates the current <see cref="SKImageInfo"/> and recreates the bitmap.
    /// </summary>
    /// <param name="info">The new image information.</param>
    public void ChangeImageInfo(SKImageInfo info) => CreateBitmap(info);

    /// <summary>
    /// Creates a bitmap based on the specified <see cref="SKImageInfo"/>.
    /// </summary>
    /// <param name="info">The image information used to create the bitmap.</param>
    private void CreateBitmap(SKImageInfo info)
    {
        if (pixels == null || !SKImageInfo.Equals(info))
        {
            FreeBitmap();

            pixels = new byte[info.BytesSize];
            pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            SKImageInfo = info;
        }
    }

    /// <summary>
    /// Frees the resources associated with the current bitmap.
    /// </summary>
    private void FreeBitmap()
    {
        if (pixels != null)
        {
            pixelsHandle.Free();
            pixels = null;
        }
    }

    /// <summary>
    /// Ensures thread safety by enforcing exclusive access to the surface.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Enter()
    {
        if (Interlocked.CompareExchange(ref _syncFlag, SYNC_ENTER, SYNC_EXIT) == SYNC_ENTER)
            Throw();
        return;
        static void Throw() => throw new InvalidOperationException();
    }

    /// <summary>
    /// Releases the thread-exclusive lock on the surface.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Exit() => Volatile.Write(ref _syncFlag, SYNC_EXIT);

    /// <summary>
    /// Releases all unmanaged resources used by the class.
    /// </summary>
    public void Dispose() => FreeBitmap();

    /// <summary>
    /// Represents an owner for the <see cref="SKSurface"/> to manage its lifecycle.
    /// </summary>
    internal readonly struct SKSurfaceOwner : IDisposable
    {
        private readonly ManagedMemorySKSurface ManagedMemorySKSurface;
        public readonly SKSurface SKSurface;

        /// <summary>
        /// Initializes a new instance of the <see cref="SKSurfaceOwner"/> struct.
        /// </summary>
        /// <param name="managedMemorySKSurface">The parent surface manager.</param>
        /// <param name="sKSurface">The surface being managed.</param>
        public SKSurfaceOwner(ManagedMemorySKSurface managedMemorySKSurface, SKSurface sKSurface)
        {
            ManagedMemorySKSurface = managedMemorySKSurface ?? throw new ArgumentNullException(nameof(managedMemorySKSurface));
            SKSurface = sKSurface ?? throw new ArgumentNullException(nameof(sKSurface));
        }

        /// <summary>
        /// Releases the resources held by the <see cref="SKSurfaceOwner"/> and exits the thread lock.
        /// </summary>
        public void Dispose()
        {
            SKSurface.Dispose();
            ManagedMemorySKSurface.Exit();
        }
    }
}
