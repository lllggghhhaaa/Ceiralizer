using System.Buffers;
using System.Text;
using Ceiralizer.Utils;

namespace Ceiralizer.Tests;

public class ChunkWriterTests
{
    private static ChunkWriter Create() => new(new ArrayBufferWriter<byte>());
    private static byte[] WriteAndGetData(Action<ChunkWriter> action)
    {
        var w = Create();
        action(w);
        return w.GetWrittenData();
    }

    #region WriteByte / WriteSByte

    [Theory]
    [InlineData(0, new byte[] { 0 })]
    [InlineData(42, new byte[] { 42 })]
    [InlineData(255, new byte[] { 255 })]
    public void WriteByte_ProducesCorrectBytes(byte value, byte[] expected)
    {
        Assert.Equal(expected, WriteAndGetData(w => w.Write(value)));
    }

    [Theory]
    [InlineData(0, new byte[] { 0 })]
    [InlineData(127, new byte[] { 127 })]
    [InlineData(-42, new byte[] { 214 })]
    public void WriteSByte_ProducesCorrectBytes(sbyte value, byte[] expected)
    {
        Assert.Equal(expected, WriteAndGetData(w => w.Write(value)));
    }

    #endregion

    #region WriteShort / WriteUShort

    [Theory]
    [InlineData((short)256, new byte[] { 0x00, 0x01 })]
    [InlineData((short)-100, new byte[] { 0x9C, 0xFF })]
    public void WriteShort_LittleEndian_ProducesCorrectBytes(short value, byte[] expected)
    {
        Assert.Equal(expected, WriteAndGetData(w => w.Write(value)));
    }

    [Theory]
    [InlineData((ushort)256, new byte[] { 0x00, 0x01 })]
    [InlineData((ushort)65535, new byte[] { 0xFF, 0xFF })]
    public void WriteUShort_LittleEndian_ProducesCorrectBytes(ushort value, byte[] expected)
    {
        Assert.Equal(expected, WriteAndGetData(w => w.Write(value)));
    }

    #endregion

    #region WriteInt / WriteUInt

    [Theory]
    [InlineData(0x12345678, new byte[] { 0x78, 0x56, 0x34, 0x12 })]
    [InlineData(-1000, new byte[] { 0x18, 0xFC, 0xFF, 0xFF })]
    public void WriteInt_LittleEndian_ProducesCorrectBytes(int value, byte[] expected)
    {
        Assert.Equal(expected, WriteAndGetData(w => w.Write(value)));
    }

    [Fact]
    public void WriteInt_MinAndMax_RoundTrip()
    {
        var data = WriteAndGetData(w => { w.Write(int.MinValue); w.Write(int.MaxValue); });
        Assert.Equal(8, data.Length);
        Assert.Equal(int.MinValue, BitConverter.ToInt32(data, 0));
        Assert.Equal(int.MaxValue, BitConverter.ToInt32(data, 4));
    }

    [Fact]
    public void WriteUInt_MaxValue_RoundTrip()
    {
        var data = WriteAndGetData(w => w.Write(uint.MaxValue));
        Assert.Equal(uint.MaxValue, BitConverter.ToUInt32(data, 0));
    }

    #endregion

    #region WriteLong / WriteULong

    [Theory]
    [InlineData(0x0123456789ABCDEFL, new byte[] { 0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01 })]
    public void WriteLong_LittleEndian_ProducesCorrectBytes(long value, byte[] expected)
    {
        Assert.Equal(expected, WriteAndGetData(w => w.Write(value)));
    }

    [Fact]
    public void WriteLong_MinAndMax_RoundTrip()
    {
        var data = WriteAndGetData(w => { w.Write(long.MinValue); w.Write(long.MaxValue); });
        Assert.Equal(long.MinValue, BitConverter.ToInt64(data, 0));
        Assert.Equal(long.MaxValue, BitConverter.ToInt64(data, 8));
    }

    [Fact]
    public void WriteULong_MaxValue_RoundTrip()
    {
        var data = WriteAndGetData(w => w.Write(ulong.MaxValue));
        Assert.Equal(ulong.MaxValue, BitConverter.ToUInt64(data, 0));
    }

    #endregion

    #region WriteFloat / WriteDouble

    [Theory]
    [InlineData(3.14f)]
    [InlineData(0f)]
    [InlineData(-1.5f)]
    [InlineData(float.MaxValue)]
    [InlineData(float.MinValue)]
    public void WriteFloat_RoundTrip_ReturnsCorrectValue(float value)
    {
        var data = WriteAndGetData(w => w.Write(value));
        Assert.Equal(value, BitConverter.ToSingle(data, 0));
    }

    [Fact]
    public void WriteFloat_SpecialValues_RoundTrip()
    {
        var data = WriteAndGetData(w => { w.Write(float.NaN); w.Write(float.PositiveInfinity); w.Write(float.NegativeInfinity); });
        Assert.True(float.IsNaN(BitConverter.ToSingle(data, 0)));
        Assert.True(float.IsPositiveInfinity(BitConverter.ToSingle(data, 4)));
        Assert.True(float.IsNegativeInfinity(BitConverter.ToSingle(data, 8)));
    }

    [Theory]
    [InlineData(2.718281828)]
    [InlineData(0.0)]
    [InlineData(-100.5)]
    public void WriteDouble_RoundTrip_ReturnsCorrectValue(double value)
    {
        var data = WriteAndGetData(w => w.Write(value));
        Assert.Equal(value, BitConverter.ToDouble(data, 0));
    }

    [Fact]
    public void WriteDouble_SpecialValues_RoundTrip()
    {
        var data = WriteAndGetData(w => { w.Write(double.NaN); w.Write(double.PositiveInfinity); });
        Assert.True(double.IsNaN(BitConverter.ToDouble(data, 0)));
        Assert.True(double.IsPositiveInfinity(BitConverter.ToDouble(data, 8)));
    }

    #endregion

    #region WriteBool

    [Theory]
    [InlineData(true, new byte[] { 1 })]
    [InlineData(false, new byte[] { 0 })]
    public void WriteBool_ProducesCorrectBytes(bool value, byte[] expected)
    {
        Assert.Equal(expected, WriteAndGetData(w => w.Write(value)));
    }

    [Fact]
    public void WriteBool_Multiple_ProducesCorrectSequence()
    {
        var data = WriteAndGetData(w => { w.Write(true); w.Write(false); w.Write(true); });
        Assert.Equal([1, 0, 1], data);
    }

    #endregion

    #region WriteString

    [Theory]
    [InlineData("Hello", "UTF8")]
    [InlineData("", "UTF8")]
    [InlineData("Hello, 世界!", "UTF8")]
    [InlineData("你好", "UTF8")]
    public void WriteString_RoundTrip_ReturnsCorrectValue(string value, string encodingName)
    {
        var encoding = encodingName switch { "UTF8" => Encoding.UTF8, "ASCII" => Encoding.ASCII, _ => Encoding.UTF8 };
        var data = WriteAndGetData(w => w.Write(value, encoding));
        Assert.Equal(value, encoding.GetString(data));
    }

    [Fact]
    public void WriteString_AsciiReplacesNonAscii()
    {
        var data = WriteAndGetData(w => w.Write("你好", Encoding.ASCII));
        Assert.Equal("??", Encoding.ASCII.GetString(data));
    }

    [Fact]
    public void WriteString_DifferentEncodings_ProducesDifferentSizes()
    {
        var utf8 = WriteAndGetData(w => w.Write("A", Encoding.UTF8));
        var unicode = WriteAndGetData(w => w.Write("A", Encoding.Unicode));
        Assert.Single(utf8);
        Assert.Equal(2, unicode.Length);
    }

    #endregion

    #region WriteChar

    [Theory]
    [InlineData('A', "ASCII", new byte[] { 0x41 })]
    [InlineData('A', "UTF8", new byte[] { 0x41 })]
    [InlineData('A', "Unicode", new byte[] { 0x41, 0x00 })]
    [InlineData('A', "UTF32", new byte[] { 0x41, 0x00, 0x00, 0x00 })]
    public void WriteChar_Encoding_ProducesCorrectBytes(char value, string encodingName, byte[] expected)
    {
        var encoding = encodingName switch
        {
            "ASCII" => Encoding.ASCII,
            "UTF8" => Encoding.UTF8,
            "Unicode" => Encoding.Unicode,
            "UTF32" => Encoding.UTF32,
            _ => Encoding.UTF8
        };
        Assert.Equal(expected, WriteAndGetData(w => w.Write(value, encoding)));
    }

    [Fact]
    public void WriteChar_Utf8Multibyte_ProducesCorrectBytes()
    {
        var data = WriteAndGetData(w => w.Write('\u20AC', Encoding.UTF8));
        Assert.Equal([0xE2, 0x82, 0xAC], data);
    }

    #endregion

    #region WriteSpan

    [Fact]
    public void WriteSpan_ProducesCorrectBytes()
    {
        Assert.Equal([0x01, 0x02, 0x03], WriteAndGetData(w => w.Write(new byte[] { 0x01, 0x02, 0x03 })));
    }

    [Fact]
    public void WriteSpan_Empty_ProducesEmpty()
    {
        Assert.Empty(WriteAndGetData(w => w.Write(ReadOnlySpan<byte>.Empty)));
    }

    #endregion

    #region Multiple Writes

    [Fact]
    public void MultipleWrites_MixedTypes_CorrectSize()
    {
        var data = WriteAndGetData(w =>
        {
            w.Write(true);
            w.Write((byte)42);
            w.Write((short)1000);
            w.Write(123456);
            w.Write(123456789L);
            w.Write(1.23f);
            w.Write(4.56);
        });
        Assert.Equal(28, data.Length);
    }

    [Fact]
    public void MultipleWrites_SequentialValues_CorrectLayout()
    {
        var data = WriteAndGetData(w => { w.Write((byte)1); w.Write((short)256); w.Write((int)65536); });
        Assert.Equal(7, data.Length);
        Assert.Equal(1, data[0]);
        Assert.Equal(0x00, data[1]);
        Assert.Equal(0x01, data[2]);
        Assert.Equal(0x00, data[3]);
        Assert.Equal(0x00, data[4]);
        Assert.Equal(0x01, data[5]);
        Assert.Equal(0x00, data[6]);
    }

    #endregion

    #region Empty Writer

    [Fact]
    public void NoWrites_ReturnsEmptyArray()
    {
        Assert.Empty(Create().GetWrittenData());
    }

    #endregion
}
