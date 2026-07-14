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

    private bool _disposed;

    /// <summary>
    /// File name (without directory path) of the open HDF5 file.
    /// </summary>
    public string Filename { get; }

    /// <summary>
    /// Native HDF5 file identifier returned by <c>H5Fopen</c>. Negative if the file failed to open.
    /// </summary>
    public long FileIdentifier { get; }

    internal HdfFile(string filepath)
    {
        H5E.set_auto(H5E.DEFAULT, null, nint.Zero);           // Turn off redundant error logging

        Filename = Path.GetFileName(filepath);

        FileIdentifier = H5F.open(filepath, H5F.ACC_RDONLY, H5P.DEFAULT);

        if (FileIdentifier < 0)
        {
            throw new IOException($"Failed to open HDF5 file: {filepath}");
        }
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
        if (H5L.iterate_by_name(FileIdentifier, groupPath, H5.index_t.NAME, H5.iter_order_t.NATIVE, ref idx, (_, name, _, _) =>
                                                                                                         {
                                                                                                             if (H5O.get_info_by_name(FileIdentifier, $"{groupPath}/{name}", out var oinfo, H5O.H5O_INFO_BASIC, H5P.DEFAULT) < 0)
                                                                                                             {
                                                                                                                 throw new IOException($"Failed to read object metadata: {groupPath}/{name}");
                                                                                                             }

                                                                                                             groupData.Add(new HdfObject
                                                                                                                           {
                                                                                                                               Name = name,
                                                                                                                               Type = s_objectTypes.TryGetValue(oinfo.type, out var objectType) ? objectType : ObjectType.Unsupported,
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

        var dataspaceId = H5D.get_space(datasetId);

        ulong totalLength;

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
                    H5D.close(datasetId);
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

        unsafe
        {
            if (H5T.get_size(nativeTypeId) != sizeof(TValue))
            {
                H5T.close(nativeTypeId);
                H5T.close(typeId);
                H5D.close(datasetId);
                return null;
            }
        }

        var buffer = new TValue[totalLength];
        var readResult = 0;
        unsafe
        {
            fixed (TValue* bufferPtr = buffer)
            {
                readResult = H5D.read(datasetId, nativeTypeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, (nint)bufferPtr);
            }
        }

        H5T.close(nativeTypeId);
        H5T.close(typeId);
        H5D.close(datasetId);

        if (readResult < 0)
        {
            // A failed read otherwise returns a zeroed buffer indistinguishable from valid data.

            throw new IOException($"Failed to read dataset: {datasetPath}");
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
    /// <returns>The string value, or <see langword="null"/> if the dataset does not exist.</returns>
    /// <remarks>The dataset must use a variable-length (<c>H5T_VARIABLE</c>) string datatype.
    /// Fixed-length string datasets are not supported and will cause undefined behaviour.</remarks>
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
    ~HdfFile() => Dispose();

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (FileIdentifier >= 0L)
        {
            H5F.close(FileIdentifier);
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Filename} | {(FileIdentifier < 0L ? "NULL" : FileIdentifier.ToString())}";
}
