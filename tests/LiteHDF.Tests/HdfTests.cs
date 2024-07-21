namespace LiteHDF.Tests
{
    using Xunit.Abstractions;

    public class HdfTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public HdfTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Should_return_libversion()
        {
            var libVersion = Hdf.GetLibraryVersion();

            _testOutputHelper.WriteLine(libVersion.ToString());
        }
    }
}