namespace LiteHDF.Internal
{
    using HDF.PInvoke;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;


    public class HdfFile : IDisposable
    {
        public delegate void GroupIterationCallback(string objectName, ObjectType type, ulong creationTimeUnixSeconds);

        private static readonly Dictionary<H5O.type_t, ObjectType> _objectTypes = new Dictionary<H5O.type_t, ObjectType>
        {
            [H5O.type_t.GROUP]   = ObjectType.Group,
            [H5O.type_t.DATASET] = ObjectType.Dataset
        };


        private readonly string _filename;

        public long FileIdentifier { get; }

        internal HdfFile(string filepath)
        {
            H5E.set_auto(H5E.DEFAULT, null, IntPtr.Zero);           // Turn off redundant error logging

            _filename = Path.GetFileName(filepath);

            FileIdentifier = H5F.open(filepath, H5F.ACC_RDONLY);
        }



        public void IterateGroup(string groupPath, GroupIterationCallback callback)
        {
            var pos = 0UL;
            H5L.iterate_by_name(FileIdentifier, groupPath, H5.index_t.NAME, H5.iter_order_t.NATIVE, ref pos, (long objectId, IntPtr namePtr, ref H5L.info_t info, IntPtr data) =>
            {
                var objectName = Marshal.PtrToStringAnsi(namePtr);

                var gInfo = new H5O.info_t();
                H5O.get_info_by_name(FileIdentifier, $"{groupPath}/{objectName}", ref gInfo);


                callback(objectName, _objectTypes[gInfo.type], gInfo.ctime);


                return 0;
            }, new IntPtr());
        }

        public HdfData<TValue> GetData<TValue>(string datasetPath)
            where TValue : unmanaged
        {
            var datasetId = H5D.open(FileIdentifier, datasetPath);

            if (datasetId < 0)
            {
                // Dataset does not exist

                return null;
            }

            var dataspaceId = H5D.get_space(datasetId);

            var rank = H5S.get_simple_extent_ndims(dataspaceId);


            var dimensionSizes = new ulong[rank];


            H5S.get_simple_extent_dims(dataspaceId, dimensionSizes, null);


            H5S.close(dataspaceId);


            var typeId = H5D.get_type(datasetId);

            var totalLength = rank > 0 ? 1UL : 0;

            for (var i = rank ; i > 0; i--)
            {
                totalLength *= dimensionSizes[i - 1];
            }

            var buffer = new TValue[totalLength];

            unsafe
            {
                fixed (TValue* bufferPtr = buffer)
                {
                    H5D.read(datasetId, typeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, (IntPtr)bufferPtr);
                }
            }

            H5T.close(typeId);
            H5D.close(datasetId);

            var info = new H5O.info_t();
            H5O.get_info_by_name(FileIdentifier, datasetPath, ref info);

            return new HdfData<TValue>(datasetPath, info.ctime == 0 ? null : info.ctime, buffer);
        }

        public string GetString(string datasetPath)
        {
            var datasetId = H5D.open(FileIdentifier, datasetPath);


            if (datasetId < 0)
            {
                // Dataset does not exist

                return null;
            }

            var datatypeId = H5D.get_type(datasetId);

            string strValue;


            unsafe
            {
                var strPtrArray = stackalloc IntPtr[1];

                H5D.read(datasetId, datatypeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, (IntPtr)strPtrArray);

                var strPtr = (byte*)strPtrArray[0];

                var strLen = 0;
                while (strPtr[strLen] != 0)
                {
                    strLen++;
                }

                strValue = Encoding.UTF8.GetString(strPtr, strLen);
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
        }


        public override string ToString() => $"{_filename} | {(FileIdentifier < 0L ? "NULL" : FileIdentifier.ToString())}" ;
    }
}
