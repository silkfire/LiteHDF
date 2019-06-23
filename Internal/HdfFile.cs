﻿namespace HdfLite.Internal
{
    using HDF.PInvoke;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;


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
            // Turn off redundant error logging

            H5E.set_auto(H5E.DEFAULT, null, IntPtr.Zero);


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


        public HdfData GetData<T>(string datasetPath)
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

            var info = new H5O.info_t();
            H5O.get_info_by_name(FileIdentifier, datasetPath, ref info);

            return new HdfData(datasetPath, info.ctime == 0 ? null as ulong? : info.ctime, dataArray);
        }


        public string GetString(string datasetPath)
        {
            var datasetId = H5D.open(FileIdentifier, datasetPath);
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
            if (FileIdentifier >= 0L)
            {
                H5F.close(FileIdentifier);
            }
        }


        public override string ToString() => $"{_filename} | {(FileIdentifier < 0L ? "NULL" : FileIdentifier.ToString())}" ;
    }
}
