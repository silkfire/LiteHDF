namespace LiteHDF.PInvoke;

using haddr_t = ulong;
using herr_t = int;
using hsize_t = ulong;
using time_t = ulong;
using hid_t = long;
using uint64_t = ulong;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security;

/// <summary>
/// Manage HDF5 objects (groups, datasets, datatype objects).
/// </summary>
internal sealed partial class H5O
{
    /// <summary>
    /// Fill in the fileno, addr, type, and rc fields.
    /// </summary>
    public const uint H5O_INFO_BASIC = 0x0001U;

    /// <summary>
    /// Fill in the atime, mtime, ctime, and btime fields.
    /// </summary>
    public const uint H5O_INFO_TIME = 0x0002U;

    /// <summary>
    /// Types of objects in file.
    /// </summary>
    public enum type_t
    {
        /// <summary>
        /// Unknown object type.
        /// </summary>
        UNKNOWN = -1,

        /// <summary>
        /// Object is a group.
        /// </summary>
        GROUP,

        /// <summary>
        /// Object is a dataset.
        /// </summary>
        DATASET,

        /// <summary>
        /// Object is a named data type.
        /// </summary>
        NAMED_DATATYPE,

        /// <summary>
        /// Number of different object types (must be last!).
        /// </summary>
        NTYPES
    }

    /// <summary>
    /// Data model information struct for objects (for <see cref="get_info_by_name"/>).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct info1_t
    {
        /// <summary>
        /// File number that object is located in.
        /// </summary>
        public uint fileno;

        /// <summary>
        /// Object address in file.
        /// </summary>
        public haddr_t addr;

        /// <summary>
        /// Basic object type (group, dataset, etc.).
        /// </summary>
        public type_t type;

        /// <summary>
        /// Reference count of object.
        /// </summary>
        public uint rc;

        /// <summary>
        /// Access time.
        /// </summary>
        public time_t atime;

        /// <summary>
        /// Modification time.
        /// </summary>
        public time_t mtime;

        /// <summary>
        /// Change time.
        /// </summary>
        public time_t ctime;

        /// <summary>
        /// Birth time.
        /// </summary>
        public time_t btime;

        /// <summary>
        /// # of attributes attached to object.
        /// </summary>
        public hsize_t num_attrs;

        /// <summary>
        /// Object header information
        /// </summary>
        public hdr_info_t hdr;

        public meta_size_t meta_size;
    }

    /// <summary>
    /// Information struct for object header metadata
    /// (for H5Oget_info/H5Oget_info_by_name/H5Oget_info_by_idx)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct hdr_info_t
    {
        /// <summary>
        /// Version number of header format in file
        /// </summary>
        public uint version;

        /// <summary>
        /// Number of object header messages
        /// </summary>
        public uint nmesgs;

        /// <summary>
        /// Number of object header chunks
        /// </summary>
        public uint nchunks;

        /// <summary>
        /// Object header status flags
        /// </summary>
        public uint flags;

        public space_t space;

        public mesg_t mesg;

        [StructLayout(LayoutKind.Sequential)]
        public struct space_t
        {
            /// <summary>
            /// Total space for storing object header in file.
            /// </summary>
            public hsize_t total;

            /// <summary>
            /// Space within header for object header metadata information.
            /// </summary>
            public hsize_t meta;

            /// <summary>
            /// Space within header for actual message information.
            /// </summary>
            public hsize_t mesg;

            /// <summary>
            /// Free space within object header.
            /// </summary>
            public hsize_t free;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct mesg_t
        {
            /// <summary>
            /// Flags to indicate presence of message type in header.
            /// </summary>
            public uint64_t present;

            /// <summary>
            /// Flags to indicate message type is shared in header.
            /// </summary>
            public uint64_t shared;
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct meta_size_t
    {
        public H5.ih_info_t obj;

        public H5.ih_info_t attr;
    }

    /// <summary>
    /// Retrieves the metadata for an object, identifying the object by location and relative name.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5O.html#Object-GetInfoByName" /> for further reference.</para>
    /// </summary>
    /// <param name="loc_id">File or group identifier specifying location of group in which object is located.</param>
    /// <param name="name">Name of object, relative to <paramref name="loc_id"/>.</param>
    /// <param name="oinfo">Buffer in which to return object information.</param>
    /// <param name="fields">Flags specifying the fields to include in <paramref name="oinfo"/>.</param>
    /// <param name="lapl_id">Link access property list.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Oget_info_by_name2"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t get_info_by_name(hid_t loc_id, [MarshalUsing(typeof(Utf8StringMarshaller))] string name, out info1_t oinfo, uint fields, hid_t lapl_id);
}
