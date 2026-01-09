namespace LiteHDF.PInvoke;

using herr_t = int;
using hid_t = long;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security;

/// <summary>
/// Manage HDF5 files.
/// </summary>
internal sealed partial class H5F
{
    // Flags for H5F.open() and H5F.create() calls

    /// <summary>
    /// Absence of rdwr => rd-only.
    /// </summary>
    public const uint ACC_RDONLY = 0x0000u;

    /// <summary>
    /// Open for read and write.
    /// </summary>
    public const uint ACC_RDWR = 0x0001u;

    /// <summary>
    /// Terminates access to an HDF5 file.
    /// https://support.hdfgroup.org/HDF5/doc/RM/RM_H5F.html#File-Close
    /// </summary>
    /// <param name="file_id">Identifier of a file to which access is terminated.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Fclose"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t close(hid_t file_id);

    /// <summary>
    /// Opens an existing HDF5 file.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5F.html#File-Open" /> for further reference.</para>
    /// </summary>
    /// <param name="filename">Name of the file to be opened.</param>
    /// <param name="flags">File access flags (<see cref="ACC_RDWR"/> or <see cref="ACC_RDONLY"/>).</param>
    /// <param name="plist">Identifier for the file access properties list.</param>
    /// <returns>Returns a file identifier if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Fopen"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial hid_t open([MarshalUsing(typeof(Utf8StringMarshaller))] string filename, uint flags, hid_t plist);
}
