namespace LiteHDF;

using PInvoke;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public sealed class HdfFile : IDisposable
{
    private static readonly Dictionary<H5O.type_t, ObjectType> s_objectTypes = new()
                                                                               {
                                                                                   [H5O.type_t.GROUP] = ObjectType.Group,
                                                                                   [H5O.type_t.DATASET] = ObjectType.Dataset
                                                                               };

    static HdfFile()
    {
        H5.open();
    }

    public string Filename { get; }

    public long FileIdentifier { get; }

    internal HdfFile(string filepath)
    {
        H5E.set_auto(H5E.DEFAULT, null, nint.Zero);           // Turn off redundant error logging

        Filename = Path.GetFileName(filepath);

        FileIdentifier = H5F.open(filepath, H5F.ACC_RDONLY, H5P.DEFAULT);
    }

    public Object[] GetGroupObjectData(string groupPath)
    {
        var idx = 0UL;
        H5L.iterate_by_name(FileIdentifier, groupPath, H5.index_t.NAME, H5.iter_order_t.NATIVE, ref idx, (_, _, _, _) => 0, nint.Zero, H5P.DEFAULT);

        var groupData = new Object[(int)idx];

        idx = 0;
        var i = 0;
        H5L.iterate_by_name(FileIdentifier, groupPath, H5.index_t.NAME, H5.iter_order_t.NATIVE, ref idx, (_, name, _, _) =>
        {
            H5O.get_info_by_name(FileIdentifier, $"{groupPath}/{name}", out var oinfo, H5O.H5O_INFO_BASIC, H5P.DEFAULT);
            groupData[i++] = new Object
                             {
                                 Name = name,
                                 Type = s_objectTypes[oinfo.type],
                                 File = this
                             };
            return 0;
        }, nint.Zero, H5P.DEFAULT);

        return groupData;
    }

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
        var dataspaceClass = H5S.get_simple_extent_type(dataspaceId);

        ulong totalLength;

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
                H5S.close(dataspaceId);

                for (var i = rank; i > 0; i--)
                {
                    totalLength *= dimensionSizes[i - 1];
                }

                break;
            default:
                return null;
        }

        var typeId = H5D.get_type(datasetId);

        var buffer = new TValue[totalLength];
        unsafe
        {
            fixed (TValue* bufferPtr = buffer)
            {
                H5D.read(datasetId, typeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, (nint)bufferPtr);
            }
        }

        H5T.close(typeId);
        H5D.close(datasetId);

        H5O.get_info_by_name(FileIdentifier, datasetPath, out var oinfo, H5O.H5O_INFO_BASIC | H5O.H5O_INFO_TIME, H5P.DEFAULT);

        return new HdfData<TValue>
               {
                   DatasetPath = datasetPath,
                   ChangeTime = oinfo.ctime == 0 ? null : oinfo.ctime,
                   Value = buffer
               };
    }

    public string? GetString(string datasetPath)
    {
        var datasetId = H5D.open(FileIdentifier, datasetPath, H5P.DEFAULT);
        if (datasetId < 0)
        {
            // Dataset does not exist

            return null;
        }

        var datatypeId = H5D.get_type(datasetId);

        string? strValue;
        unsafe
        {
            var strPtrArray = stackalloc nint[1];

            H5D.read(datasetId, datatypeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, (nint)strPtrArray);

            strValue = Marshal.PtrToStringUTF8(strPtrArray[0]);
        }

        H5T.close(datatypeId);
        H5D.close(datasetId);

        return strValue;
    }

    public void Dispose()
    {
        if (FileIdentifier >= 0L)
        {
            H5F.close(FileIdentifier);
        }

        H5.close();
    }

    public override string ToString() => $"{Filename} | {(FileIdentifier < 0L ? "NULL" : FileIdentifier.ToString())}";
}
