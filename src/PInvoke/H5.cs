namespace LiteHDF.PInvoke;

using herr_t = int;
using hsize_t = ulong;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

/// <summary>
/// Manage the life cycle of HDF5 library instances.
/// </summary>
internal sealed partial class H5
{
    /// <summary>
    /// Common iteration orders.
    /// </summary>
    public enum iter_order_t
    {
        /// <summary>
        /// Unknown order.
        /// </summary>
        UNKNOWN = -1,

        /// <summary>
        /// Increasing order.
        /// </summary>
        INC = 0,

        /// <summary>
        /// Decreasing order.
        /// </summary>
        DEC = 1,

        /// <summary>
        /// No particular order, whatever is fastest.
        /// </summary>
        NATIVE = 2,

        /// <summary>
        /// Number of iteration orders.
        /// </summary>
        N = 3
    }

    /// <summary>
    /// The types of indices on links in groups/attributes on objects.
    /// Primarily used for "[do] [foo] by index" routines and for iterating
    /// over links in groups/attributes on objects.
    /// </summary>
    public enum index_t
    {
        /// <summary>
        /// Unknown index type.
        /// </summary>
        UNKNOWN = -1,

        /// <summary>
        /// Index on names.
        /// </summary>
        NAME,

        /// <summary>
        /// Index on creation order.
        /// </summary>
        CRT_ORDER,

        /// <summary>
        /// Number of indices defined.
        /// </summary>
        N
    }

    /// <summary>
    /// Storage info struct used by <see cref="H5O.info1_t"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ih_info_t
    {
        public hsize_t heap_size;

        /// <summary>
        /// btree and/or list
        /// </summary>
        public hsize_t index_size;
    }

    /// <summary>
    /// Flushes all data to disk, closes all open identifiers, and cleans up memory.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5.html#Library-Close" /> for further reference.</para>
    /// </summary>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5close"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t close();

    /// <summary>
    /// Initializes the HDF5 library.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5.html#Library-Open" /> for further reference.</para>
    /// </summary>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5open"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t open();

    /// <summary>
    /// Frees memory allocated by the HDF5 library.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5.html#Library-FreeMemory" /> for further reference.</para>
    /// </summary>
    /// <param name="mem">Buffer to be freed. Can be <see langword="null"/>.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5free_memory"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t free_memory(nint mem);

    /// <summary>
    /// Returns the HDF library release number.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5.html#Library-Version" /> for further reference.</para>
    /// </summary>
    /// <param name="majnum">The major version of the library.</param>
    /// <param name="minnum">The minor version of the library.</param>
    /// <param name="relnum">The release number of the library.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5get_libversion"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t get_libversion(ref uint majnum, ref uint minnum, ref uint relnum);
}
