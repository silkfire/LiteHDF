namespace HdfLite
{
    using Objects;


    public static class Hdf
    {
        public static HdfFile Open(string filename)
        {
            return new HdfFile(filename);
        }
    }
}
