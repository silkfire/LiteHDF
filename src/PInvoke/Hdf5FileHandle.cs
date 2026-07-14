namespace LiteHDF.PInvoke;

using System;
using System.Runtime.InteropServices;

using hid_t = long;

/// <summary>
/// A <see cref="SafeHandle"/> wrapping an open HDF5 file identifier (<c>hid_t</c>), closing it
/// via <c>H5Fclose</c> when released.
/// </summary>
/// <remarks>HDF5 identifiers are negative on failure, so any negative value is treated as invalid.</remarks>
internal sealed class Hdf5FileHandle : SafeHandle
{
    public Hdf5FileHandle(hid_t fileId)
        : base(unchecked((nint)(-1)), ownsHandle: true)
    {
        SetHandle((nint)fileId);
    }

    /// <inheritdoc/>
    public override bool IsInvalid => (hid_t)handle < 0;

    /// <summary>
    /// The underlying HDF5 file identifier.
    /// </summary>
    public hid_t FileId => (hid_t)handle;

    /// <inheritdoc/>
    protected override bool ReleaseHandle() => H5F.close((hid_t)handle) >= 0;
}
