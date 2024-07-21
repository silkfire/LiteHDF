namespace LiteHDF
{
    using PInvoke;

    using System;

    public static class Hdf
    {
        public static HdfFile Open(string filename)
        {
            return new HdfFile(filename);
        }

        /// <summary>
        /// Gets the version of the underlying HD5 library.
        /// </summary>
        public static Version GetLibraryVersion()
        {
            uint majorVersion = 0U, minorVersion = 0U, releaseVersion = 0U;
            H5.get_libversion(ref majorVersion, ref minorVersion, ref releaseVersion);

            var libraryVersion = new Version((int)majorVersion, (int)minorVersion, (int)releaseVersion);

            return libraryVersion;
        }
    }
}
