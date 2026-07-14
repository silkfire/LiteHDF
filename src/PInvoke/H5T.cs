namespace LiteHDF.PInvoke;

using herr_t = int;
using htri_t = int;
using hid_t = long;
using size_t = nint;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

/// <summary>
/// HDF5 datatypes describe the element type of HDF5 datasets and attributes.
/// </summary>
internal static partial class H5T
{
    /// <summary>
    /// The order to retrieve atomic native datatype.
    /// </summary>
    public enum direction_t
    {
        /// <summary>
        /// Default direction is ascending.
        /// </summary>
        DEFAULT = 0,

        /// <summary>
        /// In ascending order.
        /// </summary>
        ASCEND = 1,

        /// <summary>
        /// In descending order.
        /// </summary>
        DESCEND = 2
    }

    /// <summary>
    /// Character set to use for text strings.
    /// </summary>
    public enum cset_t
    {
        /// <summary>
        /// Error.
        /// </summary>
        ERROR = -1,

        /// <summary>
        /// US ASCII.
        /// </summary>
        ASCII = 0,

        /// <summary>
        /// UTF-8 Unicode encoding.
        /// </summary>
        UTF8 = 1,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        RESERVED_2 = 2,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_3 = 3,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_4 = 4,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_5 = 5,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_6 = 6,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_7 = 7,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_8 = 8,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_9 = 9,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_10 = 10,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_11 = 11,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_12 = 12,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_13 = 13,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_14 = 14,

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        RESERVED_15 = 15,
    }

    /// <summary>
    /// Releases a datatype.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5T.html#Datatype-Close" /> for further reference.</para>
    /// </summary>
    /// <param name="type_id">Identifier of datatype to release.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Tclose"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t close(hid_t type_id);

    /// <summary>
    /// Returns the size of a datatype in bytes.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5T.html#Datatype-GetSize" /> for further reference.</para>
    /// </summary>
    /// <param name="type_id">Identifier of datatype to query.</param>
    /// <returns>Returns the size of the datatype in bytes if successful; otherwise returns 0.</returns>
    // SuppressGCTransition: pure in-memory getter (reads the size off an already-loaded
    // datatype). No I/O, no allocation, no callback, and the bundled hdf5.dll is a
    // non-threadsafe build (no library mutex to acquire), so skipping the GC transition
    // is safe. If the DLL is ever swapped for a threadsafe build, remove this attribute.
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Tget_size"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), SuppressGCTransition]
    public static partial size_t get_size(hid_t type_id);

    /// <summary>
    /// Returns the native datatype identifier of a specified datatype.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5T.html#Datatype-GetNativeType" /> for further reference.</para>
    /// </summary>
    /// <param name="type_id">Identifier of datatype to query.</param>
    /// <param name="direction">Direction of search.</param>
    /// <returns>Returns the native datatype identifier if successful; otherwise returns a negative value.</returns>
    /// <remarks>The returned datatype must be released with <see cref="close"/>.</remarks>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Tget_native_type"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial hid_t get_native_type(hid_t type_id, direction_t direction);

    /// <summary>
    /// Determines whether a datatype is a variable-length string.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5T.html#Datatype-IsVariableString" /> for further reference.</para>
    /// </summary>
    /// <param name="type_id">Identifier of datatype to query.</param>
    /// <returns>Returns a positive value if the datatype is a variable-length string, zero if it is not, and a negative value on error.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Tis_variable_str"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial htri_t is_variable_str(hid_t type_id);
}
