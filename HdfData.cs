namespace HdfLite
{
    using System;


    public class HdfData
    {
        public string DatasetPath { get; }

        public ulong? ChangeTime { get; }

        public Array Value { get; }



        public HdfData(string datasetPath, ulong? changeTime, Array value)
        {
            DatasetPath = datasetPath;
            ChangeTime = changeTime;
            Value = value;
        }


        public override string ToString() => DatasetPath;
    }
}
