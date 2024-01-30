namespace LiteHDF.PInvoke;

using herr_t = int;
using hid_t = long;

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
    }

    /// <summary>
    /// Releases a datatype.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5T.html#Datatype-Close" /> for further reference.</para>
    /// </summary>
    /// <param name="type_id">Identifier of datatype to release.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HD5LibraryName, EntryPoint = "H5Tclose"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t close(hid_t type_id);
}
