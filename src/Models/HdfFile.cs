namespace LiteHDF;

using PInvoke;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Represents an open HDF5 file. Dispose when done to release the native file handle.
/// </summary>
public sealed class HdfFile : IDisposable
{
    private static readonly ReadOnlyDictionary<H5O.type_t, ObjectType> s_objectTypes = new Dictionary<H5O.type_t, ObjectType>
                                                                                       {
                                                                                           [H5O.type_t.GROUP] = ObjectType.Group,
                                                                                           [H5O.type_t.DATASET] = ObjectType.Dataset
                                                                                       }.AsReadOnly();

    private readonly Hdf5FileHandle _handle;

    static HdfFile()
    {
        // Turn off HDF5's automatic error printing. This mutates process-global state,
        // so do it once per process rather than on every file open.
        H5E.set_auto(H5E.DEFAULT, null, nint.Zero);
    }

    /// <summary>
    /// File name (without directory path) of the open HDF5 file.
    /// </summary>
    public string Filename { get; }

    /// <summary>
    /// Native HDF5 file identifier returned by <c>H5Fopen</c>.
    /// </summary>
    public long FileIdentifier => _handle.FileId;

    internal HdfFile(string filepath)
    {
        Filename = Path.GetFileName(filepath);

        var fileId = H5F.open(filepath, H5F.ACC_RDONLY, H5P.DEFAULT);

        if (fileId < 0)
        {
            throw new IOException($"Failed to open HDF5 file: {filepath}");
        }

        _handle = new Hdf5FileHandle(fileId);
    }

    /// <summary>
    /// Returns metadata for all objects (groups and datasets) directly within a group.
    /// </summary>
    /// <param name="groupPath">Absolute path to the group within the file.</param>
    /// <returns>An array of <see cref="HdfObject"/> describing each child object.</returns>
    public HdfObject[] GetGroupObjectData(string groupPath)
    {
        List<HdfObject> groupData = [];

        var idx = 0UL;
        if (H5L.iterate_by_name(FileIdentifier, groupPath, H5.index_t.NAME, H5.iter_order_t.NATIVE, ref idx, (_, name, in _, _) =>
                                                                                                         {
                                                                                                             if (H5O.get_info_by_name(FileIdentifier, $"{groupPath}/{name}", out var oinfo, H5O.H5O_INFO_BASIC, H5P.DEFAULT) < 0)
                                                                                                             {
                                                                                                                 throw new IOException($"Failed to read object metadata: {groupPath}/{name}");
                                                                                                             }

                                                                                                             groupData.Add(new HdfObject
                                                                                                                           {
                                                                                                                               Name = name,
                                                                                                                               Type = s_objectTypes.GetValueOrDefault(oinfo.type, ObjectType.Unsupported),
                                                                                                                               File = this
                                                                                                                           });
                                                                                                             return 0;
                                                                                                         }, nint.Zero, H5P.DEFAULT) < 0)
        {
            // A negative return distinguishes a nonexistent group from an empty one.

            throw new IOException($"Failed to iterate group: {groupPath}");
        }

        return groupData.ToArray();
    }

    /// <summary>
    /// Reads a numeric dataset into a typed array.
    /// </summary>
    /// <typeparam name="TValue">Unmanaged element type. Must match the byte size of the file's datatype.</typeparam>
    /// <param name="datasetPath">Absolute path to the dataset within the file.</param>
    /// <returns>
    /// An <see cref="HdfData{TValue}"/> containing the data, or <see langword="null"/> if the dataset does not exist,
    /// has an unsupported dataspace class, or <typeparamref name="TValue"/> does not match the file datatype's element size.
    /// </returns>
    public HdfData<TValue>? GetData<TValue>(string datasetPath)
        where TValue : unmanaged
    {
        var datasetId = H5D.open(FileIdentifier, datasetPath, H5P.DEFAULT);
        if (datasetId < 0)
        {
            // Dataset does not exist

            return null;
        }

        TValue[] buffer;

        try
        {
            ulong totalLength;

            var dataspaceId = H5D.get_space(datasetId);

            try
            {
                var dataspaceClass = H5S.get_simple_extent_type(dataspaceId);

                switch (dataspaceClass)
                {
                    case H5S.class_t.NULL:
                        totalLength = 0;
                        break;
                    case H5S.class_t.SCALAR:
                        totalLength = 1;
                        break;
                    case H5S.class_t.SIMPLE:
                        totalLength = 1;

                        var rank = H5S.get_simple_extent_ndims(dataspaceId);
                        if (rank == 0)
                        {
                            rank = 1;
                        }

                        var dimensionSizes = new ulong[rank];

                        H5S.get_simple_extent_dims(dataspaceId, dimensionSizes, null);

                        for (var i = rank; i > 0; i--)
                        {
                            totalLength *= dimensionSizes[i - 1];
                        }

                        break;
                    case H5S.class_t.NO_CLASS:
                    default:
                        return null;
                }
            }
            finally
            {
                H5S.close(dataspaceId);
            }

            var typeId = H5D.get_type(datasetId);

            // Read into the native in-memory representation of the file's datatype rather than
            // the file datatype itself. Passing the file type as the memory type suppresses all
            // conversion, so a big-endian (or otherwise non-native) file would read as
            // byte-swapped garbage.
            var nativeTypeId = H5T.get_native_type(typeId, H5T.direction_t.DEFAULT);

            try
            {
                unsafe
                {
                    if (H5T.get_size(nativeTypeId) != sizeof(TValue))
                    {
                        return null;
                    }
                }

                buffer = new TValue[totalLength];

                // A NULL dataspace reads zero elements; a fixed on an empty array yields a
                // null pointer, so skip the read entirely rather than pass H5Dread a null buffer.
                if (totalLength > 0)
                {
                    int readResult;
                    unsafe
                    {
                        fixed (TValue* bufferPtr = buffer)
                        {
                            readResult = H5D.read(datasetId, nativeTypeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, (nint)bufferPtr);
                        }
                    }

                    if (readResult < 0)
                    {
                        // A failed read otherwise returns a zeroed buffer indistinguishable from valid data.

                        throw new IOException($"Failed to read dataset: {datasetPath}");
                    }
                }
            }
            finally
            {
                H5T.close(nativeTypeId);
                H5T.close(typeId);
            }
        }
        finally
        {
            H5D.close(datasetId);
        }

        if (H5O.get_info_by_name(FileIdentifier, datasetPath, out var oinfo, H5O.H5O_INFO_BASIC | H5O.H5O_INFO_TIME, H5P.DEFAULT) < 0)
        {
            throw new IOException($"Failed to read dataset metadata: {datasetPath}");
        }

        return new HdfData<TValue>
               {
                   DatasetPath = datasetPath,
                   ChangeTime = oinfo.ctime == 0 ? null : oinfo.ctime,
                   Value = buffer
               };
    }

    /// <summary>
    /// Reads a variable-length UTF-8 string dataset.
    /// </summary>
    /// <param name="datasetPath">Absolute path to the dataset within the file.</param>
    /// <returns>The string value, or <see langword="null"/> if the dataset does not exist, is not a
    /// single-element dataspace, or does not use a variable-length string datatype.</returns>
    /// <remarks>Only a single variable-length (<c>H5T_VARIABLE</c>) string element is supported.
    /// Multi-element and fixed-length string datasets return <see langword="null"/>.</remarks>
    public string? GetString(string datasetPath)
    {
        var datasetId = H5D.open(FileIdentifier, datasetPath, H5P.DEFAULT);
        if (datasetId < 0)
        {
            // Dataset does not exist

            return null;
        }

        var datatypeId = H5D.get_type(datasetId);
        var dataspaceId = H5D.get_space(datasetId);

        try
        {
            // Only a single variable-length string element is supported. A multi-element
            // dataspace would make H5Dread write one pointer per element into the single
            // buffer slot below (a buffer overrun); a fixed-length string isn't a pointer
            // at all. Guard against both instead of corrupting memory.

            if (H5S.get_simple_extent_npoints(dataspaceId) != 1 || H5T.is_variable_str(datatypeId) <= 0)
            {
                return null;
            }

            var strPtr = nint.Zero;

            if (H5D.read(datasetId, datatypeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, ref strPtr) < 0)
            {
                throw new IOException($"Failed to read string dataset: {datasetPath}");
            }

            var strValue = Marshal.PtrToStringUTF8(strPtr);

            H5.free_memory(strPtr);

            return strValue;
        }
        finally
        {
            H5S.close(dataspaceId);
            H5T.close(datatypeId);
            H5D.close(datasetId);
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _handle.Dispose();

    /// <inheritdoc/>
    public override string ToString() => $"{Filename} | {(_handle.IsClosed || _handle.IsInvalid ? "NULL" : FileIdentifier.ToString())}";
}
