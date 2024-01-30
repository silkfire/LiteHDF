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
internal sealed partial class H5L
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
            ///// <summary>
            ///// Token of location that hard link points to.
            ///// </summary>
            //[FieldOffset(0)]
            //public H5O.token_t token;

            [FieldOffset(0)]
            public byte token_data1;

            [FieldOffset(1)]
            public byte token_data2;

            [FieldOffset(2)]
            public byte token_data3;

            [FieldOffset(3)]
            public byte token_data4;

            [FieldOffset(4)]
            public byte token_data5;

            [FieldOffset(5)]
            public byte token_data6;

            [FieldOffset(6)]
            public byte token_data7;

            [FieldOffset(7)]
            public byte token_data8;

            [FieldOffset(8)]
            public byte token_data9;

            [FieldOffset(9)]
            public byte token_data10;

            [FieldOffset(10)]
            public byte token_data11;

            [FieldOffset(11)]
            public byte token_data12;

            [FieldOffset(12)]
            public byte token_data13;

            [FieldOffset(13)]
            public byte token_data14;

            [FieldOffset(14)]
            public byte token_data15;

            [FieldOffset(15)]
            public byte token_data16;

            /// <summary>
            /// Size of a soft link or user-defined link value.
            /// </summary>
            [FieldOffset(0)]
            public size_t val_size;

            // TODO: Get token_t
        }
    }

    /// <summary>
    /// Prototype for <see cref="iterate_by_name(hid_t,string,H5.index_t,H5.iter_order_t,ref hsize_t,iterate2_t,nint,hid_t)"/> operator.
    /// </summary>
    /// <param name="group">Group that serves as root of the iteration.</param>
    /// <param name="name">Name of link, relative to <paramref name="group"/>, being examined at current step of the iteration.</param>
    /// <param name="info">An <see cref="info2_t"/> struct containing information regarding that link.</param>
    /// <param name="op_data">User-defined pointer to data required by the application in processing the link.</param>
    /// <returns>Zero causes the visit iterator to continue, returning zero when all group members have been processed. A positive value causes the visit iterator to immediately return that positive value, indicating short-circuit success. A negative value causes the visit iterator to immediately return that value, indicating failure.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate herr_t iterate2_t(hid_t group, [MarshalUsing(typeof(AnsiStringMarshaller))] string name, info2_t info, nint op_data);

    public static string[] GetGroupLinks(hid_t loc_id, string groupName)
    {
        var idx = 0UL;
        iterate_by_name(loc_id, groupName, H5.index_t.NAME, H5.iter_order_t.NATIVE, ref idx, (_, _, _, _) => 0, nint.Zero, H5P.DEFAULT);

        var links = new string[(int)idx];
        idx = 0;
        int i = 0;
        iterate_by_name(loc_id, groupName, H5.index_t.NAME, H5.iter_order_t.NATIVE, ref idx, (_, name, _, _) =>
        {
            links[i++] = name;

            return 0;
        }, nint.Zero, H5P.DEFAULT);

        return links;
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
    /// <param name="op">Callback function passing data regarding the link to the calling application.</param>
    /// <param name="op_data">User-defined pointer to data required by the application for its processing of the link.</param>
    /// <param name="lapl_id">Link access property list.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HD5LibraryName, EntryPoint = "H5Literate_by_name2"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t iterate_by_name(hid_t loc_id, [MarshalUsing(typeof(AnsiStringMarshaller))] string group_name, H5.index_t idx_type, H5.iter_order_t order, ref hsize_t idx, iterate2_t op, nint op_data, hid_t lapl_id);

    
}
