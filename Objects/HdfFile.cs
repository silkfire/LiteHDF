namespace HdfLite.Objects
{
    using Enums;

    using HDF.PInvoke;

    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;


    public class HdfFile : IDisposable
    {
        private readonly long _fileIdentifier;

        private static readonly Dictionary<H5O.type_t, ObjectType> _objectTypes = new Dictionary<H5O.type_t, ObjectType>
        {
            [H5O.type_t.GROUP]   = ObjectType.Group,
            [H5O.type_t.DATASET] = ObjectType.Dataset
        };


        internal HdfFile(string filename)
        {
            _fileIdentifier = H5F.open(filename, H5F.ACC_RDONLY);
        }



        public void IterateGroup(string groupPath, Action<string, ObjectType, ulong> operation)
        {
            var pos = 0UL;
            H5L.iterate_by_name(_fileIdentifier, groupPath, H5.index_t.NAME, H5.iter_order_t.NATIVE, ref pos, (long objectId, IntPtr namePtr, ref H5L.info_t info, IntPtr data) =>
            {
                var objectName = Marshal.PtrToStringAnsi(namePtr);

                var gInfo = new H5O.info_t();
                H5O.get_info_by_name(_fileIdentifier, $"{groupPath}/{objectName}", ref gInfo);


                operation(objectName, _objectTypes[gInfo.type], gInfo.ctime);


                return 0;
            }, new IntPtr());
        }


        public Array GetData<T>(string datasetPath)
        {
            var datasetId   = H5D.open(_fileIdentifier, datasetPath);
            var dataspaceId = H5D.get_space(datasetId);

            var rank = H5S.get_simple_extent_ndims(dataspaceId);


            var dimensionSizes = new ulong[rank];

            H5S.get_simple_extent_dims(dataspaceId, dimensionSizes, null);


            //ulong[] dimensionSizes;

            //if (rank == 0)
            //{
            //    //rank         =             1;
            //    dimensionSizes = new[] { 1UL };
            //}
            //else
            //{
            //    dimensionSizes = new ulong[rank];

            //    H5S.get_simple_extent_dims(dataspaceId, dimensionSizes, null);
            //}



            H5S.close(dataspaceId);



            var dataArray = Array.CreateInstance(typeof(T), Array.ConvertAll(dimensionSizes, ds => (long)ds));

            var arrayHandle = GCHandle.Alloc(dataArray, GCHandleType.Pinned);


            var typeId = H5D.get_type(datasetId);


            H5D.read(datasetId, typeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, arrayHandle.AddrOfPinnedObject());

            arrayHandle.Free();

            H5T.close(typeId);
            H5D.close(datasetId);


            return dataArray;
        }


        public string GetString(string datasetPath)
        {
            var datasetId = H5D.open(_fileIdentifier, datasetPath);
            var typeId    = H5D.get_type(datasetId);

            Func<IntPtr, string> ptrToString;

            var characterSet = H5T.get_cset(typeId);

            switch (characterSet)
            {
                case H5T.cset_t.ASCII:
                    ptrToString = Marshal.PtrToStringAnsi;

                    break;
                case H5T.cset_t.UTF8:
                    ptrToString = Marshal.PtrToStringUTF8;

                    break;
                default:
                    throw new Exception($"Failed to parse string due to unsupported character set: {characterSet}");
            }


            var strPtrArray = new IntPtr[1];

            var strHandle = GCHandle.Alloc(strPtrArray, GCHandleType.Pinned);
            H5D.read(datasetId, typeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, strHandle.AddrOfPinnedObject());

            var strValue = ptrToString(strPtrArray[0]);

            H5.free_memory(strPtrArray[0]);

            strHandle.Free();

            H5T.close(typeId);
            H5D.close(datasetId);


            return strValue;
        }







        public void Dispose()
        {
            if (_fileIdentifier != 0L)
            {
                H5F.close(_fileIdentifier);
            }
        }
    }
}
