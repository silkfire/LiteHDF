namespace LiteHDF
{
    public class HdfData<TValue>
    {
        public string DatasetPath { get; }

        public ulong? ChangeTime { get; }

        public TValue[] Value { get; }



        public HdfData(string datasetPath, ulong? changeTime, TValue[] value)
        {
            DatasetPath = datasetPath;
            ChangeTime = changeTime;
            Value = value;
        }


        public override string ToString() => DatasetPath;
    }
}
