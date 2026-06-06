namespace LiteHDF.Tests;

public class GetDataTests
{
    // ------------------------------------------------------------------
    // Numeric type coverage — exact values for each element width
    // ------------------------------------------------------------------

    [Fact]
    public void GetData_int8_returns_correct_values()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<sbyte>("/i8");

        Assert.NotNull(data);
        Assert.Equal([unchecked((sbyte)-128), -1, 0, 127], data.Value);
    }

    [Fact]
    public void GetData_int16_returns_correct_values()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<short>("/i16");

        Assert.NotNull(data);
        Assert.Equal([-32768, -1, 0, 32767], data.Value);
    }

    [Fact]
    public void GetData_int32_returns_correct_values()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<int>("/i32");

        Assert.NotNull(data);
        Assert.Equal([int.MinValue, -1, 0, int.MaxValue], data.Value);
    }

    [Fact]
    public void GetData_int64_returns_correct_values()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<long>("/i64");

        Assert.NotNull(data);
        Assert.Equal([long.MinValue, -1L, 0L, long.MaxValue], data.Value);
    }

    [Fact]
    public void GetData_uint8_returns_correct_values()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<byte>("/u8");

        Assert.NotNull(data);
        Assert.Equal([0, 1, 127, 255], data.Value);
    }

    [Fact]
    public void GetData_uint16_returns_correct_values()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<ushort>("/u16");

        Assert.NotNull(data);
        Assert.Equal([0, 1, 32767, 65535], data.Value);
    }

    [Fact]
    public void GetData_uint32_returns_correct_values()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<uint>("/u32");

        Assert.NotNull(data);
        Assert.Equal([0u, 1u, 2147483647u, 4294967295u], data.Value);
    }

    [Fact]
    public void GetData_uint64_returns_correct_values()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<ulong>("/u64");

        Assert.NotNull(data);
        Assert.Equal([0UL, 1UL, 9223372036854775807UL, 18446744073709551615UL], data.Value);
    }

    [Fact]
    public void GetData_float32_returns_correct_values()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<float>("/f32");

        Assert.NotNull(data);
        Assert.Equal(4, data.Value.Length);
        Assert.Equal(-1.5f, data.Value[0]);
        Assert.Equal(0.0f,  data.Value[1]);
        Assert.Equal(1.5f,  data.Value[2]);
        Assert.Equal(3.14f, data.Value[3], precision: 5);
    }

    [Fact]
    public void GetData_float64_returns_correct_values()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<double>("/f64");

        Assert.NotNull(data);
        Assert.Equal(4, data.Value.Length);
        Assert.Equal(-1.5,                data.Value[0]);
        Assert.Equal(0.0,                 data.Value[1]);
        Assert.Equal(1.5,                 data.Value[2]);
        Assert.Equal(3.141592653589793,   data.Value[3]);
    }

    // ------------------------------------------------------------------
    // Element-size mismatch → null
    // ------------------------------------------------------------------

    [Fact]
    public void GetData_wrong_size_float_on_double_returns_null()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);

        // float is 4 bytes, f64 dataset has 8-byte elements
        Assert.Null(hdf.GetData<float>("/f64"));
    }

    [Fact]
    public void GetData_wrong_size_int_on_int64_returns_null()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);

        Assert.Null(hdf.GetData<int>("/i64"));
    }

    [Fact]
    public void GetData_wrong_size_short_on_int32_returns_null()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);

        Assert.Null(hdf.GetData<short>("/i32"));
    }

    [Fact]
    public void GetData_wrong_size_double_on_int32_returns_null()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);

        Assert.Null(hdf.GetData<double>("/i32"));
    }

    // ------------------------------------------------------------------
    // Same element size, different signedness — size-only check means non-null
    // (data is reinterpreted bit-for-bit; we only assert non-null + length)
    // ------------------------------------------------------------------

    [Fact]
    public void GetData_uint_on_int32_same_size_returns_non_null()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<uint>("/i32");

        Assert.NotNull(data);
        Assert.Equal(4, data.Value.Length);
    }

    // ------------------------------------------------------------------
    // Dataspace shape coverage
    // ------------------------------------------------------------------

    [Fact]
    public void GetData_scalar_dataspace_returns_single_element()
    {
        using var hdf = Hdf.Open(TestFiles.Shapes);
        var data = hdf.GetData<int>("/scalar");

        Assert.NotNull(data);
        Assert.Single(data.Value);
        Assert.Equal(42, data.Value[0]);
    }

    [Fact]
    public void GetData_vector_dataspace_returns_all_elements()
    {
        using var hdf = Hdf.Open(TestFiles.Shapes);
        var data = hdf.GetData<double>("/vector");

        Assert.NotNull(data);
        Assert.Equal(5, data.Value.Length);
        Assert.Equal([1.0, 2.0, 3.0, 4.0, 5.0], data.Value);
    }

    [Fact]
    public void GetData_2d_matrix_returns_row_major_flat_array()
    {
        using var hdf = Hdf.Open(TestFiles.Shapes);
        var data = hdf.GetData<int>("/matrix");

        Assert.NotNull(data);
        Assert.Equal(6, data.Value.Length);
        // Row-major: [[1,2,3],[4,5,6]] → [1,2,3,4,5,6]
        Assert.Equal([1, 2, 3, 4, 5, 6], data.Value);
    }

    [Fact]
    public void GetData_3d_cube_returns_row_major_flat_array()
    {
        using var hdf = Hdf.Open(TestFiles.Shapes);
        var data = hdf.GetData<int>("/cube");

        Assert.NotNull(data);
        Assert.Equal(8, data.Value.Length);
        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8], data.Value);
    }

    [Fact]
    public void GetData_null_dataspace_returns_empty_array()
    {
        using var hdf = Hdf.Open(TestFiles.Shapes);
        var data = hdf.GetData<int>("/empty");

        Assert.NotNull(data);
        Assert.Empty(data.Value);
    }

    // ------------------------------------------------------------------
    // Missing dataset → null
    // ------------------------------------------------------------------

    [Fact]
    public void GetData_missing_dataset_returns_null()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);

        Assert.Null(hdf.GetData<int>("/does_not_exist"));
    }

    // ------------------------------------------------------------------
    // ChangeTime — h5py-authored files typically record ctime = 0 → null
    // ------------------------------------------------------------------

    [Fact]
    public void GetData_change_time_is_null_for_h5py_file()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<int>("/i32");

        Assert.NotNull(data);
        // h5py doesn't record object change time; library maps ctime=0 to null
        Assert.Null(data.ChangeTime);
    }

    // ------------------------------------------------------------------
    // HdfData<T> metadata
    // ------------------------------------------------------------------

    [Fact]
    public void GetData_dataset_path_round_trips()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<int>("/i32");

        Assert.NotNull(data);
        Assert.Equal("/i32", data.DatasetPath);
    }

    [Fact]
    public void GetData_ToString_returns_dataset_path()
    {
        using var hdf = Hdf.Open(TestFiles.Numeric);
        var data = hdf.GetData<int>("/i32");

        Assert.NotNull(data);
        Assert.Equal("/i32", data.ToString());
    }
}
