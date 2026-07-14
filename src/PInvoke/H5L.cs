namespace LiteHDF.PInvoke;

using herr_t = int;
using hsize_t = ulong;
using size_t = nint;
using hid_t = long;
using int64_t = long;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security;

/// <summary>
/// Manage HDF5 links and link types.
/// </summary>
internal static partial class H5L
{
    /// <summary>
    /// Link class types.
    /// <para>Values less than 64 are reserved for the HDF5 library's internal use.<br/>
    /// Values 64 to 255 are for "user-defined" link class types; these types are defined by HDF5 but their behavior can be overridden by users.<br/>
    /// Users who want to create new classes of links should contact the HDF5 development team at <see href="hdfhelp@hdfgroup.org" />.</para>
    /// <para>These values can never change because they appear in HDF5 files.</para>
    /// </summary>
    public enum type_t
    {
        /// <summary>
        /// Invalid link type.
        /// </summary>
        ERROR = -1,

        /// <summary>
        /// Hard link.
        /// </summary>
        HARD = 0,

        /// <summary>
        /// Soft link.
        /// </summary>
        SOFT = 1,

        /// <summary>
        /// External link.
        /// </summary>
        EXTERNAL = 64,

        /// <summary>
        /// Maximum link type.
        /// </summary>
        MAX = 255
    }

    /// <summary>
    /// Information struct for links.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct info2_t
    {
        /// <summary>
        /// Type of link.
        /// </summary>
        public type_t type;

        /// <summary>
        /// Indicates if creation order is valid.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool corder_valid;

        /// <summary>
        /// Creation order.
        /// </summary>
        public int64_t corder;

        /// <summary>
        /// Character set of link name.
        /// </summary>
        public H5T.cset_t cset;

        /// <summary>
        /// Address to which hard link points or size of a soft link or UD link value.
        /// </summary>
        public u_t u;

        [StructLayout(LayoutKind.Explicit)]
        public struct u_t
        {
            /// <summary>
            /// Token of location that hard link points to.
            /// </summary>
            [FieldOffset(0)]
            public H5O.token_t token;

            /// <summary>
            /// Size of a soft link or user-defined link value.
            /// </summary>
            [FieldOffset(0)]
            public size_t val_size;
        }
    }

    /// <summary>
    /// Iterates through links in a group.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5L.html#Link-IterateByName" /> for further reference.</para>
    /// </summary>
    /// <param name="loc_id">File or group identifier specifying location of subject group.</param>
    /// <param name="group_name">Name of subject group.</param>
    /// <param name="idx_type">Type of index which determines the order.</param>
    /// <param name="order">Order within index.</param>
    /// <param name="idx">Iteration position at which to start.</param>
    /// <param name="op">
    /// Unmanaged cdecl callback invoked once per link, mirroring the C prototype
    /// <c>herr_t (*H5L_iterate2_t)(hid_t group, const char *name, const H5L_info2_t *info, void *op_data)</c>:
    /// <c>group</c> is the iteration-root group and <c>name</c> (a raw UTF-8 <c>byte*</c>) is relative to it;
    /// <c>info</c> is an <see cref="info2_t"/>*. It must be a <c>[UnmanagedCallersOnly]</c> static method and must not
    /// let a managed exception escape into the native frame. Return zero to continue, a positive value for
    /// short-circuit success, or a negative value to abort with failure.
    /// </param>
    /// <param name="op_data">User-defined pointer to data required by the application for its processing of the link.</param>
    /// <param name="lapl_id">Link access property list.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Literate_by_name2"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial herr_t iterate_by_name(hid_t loc_id, [MarshalUsing(typeof(Utf8StringMarshaller))] string group_name, H5.index_t idx_type, H5.iter_order_t order, ref hsize_t idx, delegate* unmanaged[Cdecl]<hid_t, byte*, info2_t*, nint, herr_t> op, nint op_data, hid_t lapl_id);
}
