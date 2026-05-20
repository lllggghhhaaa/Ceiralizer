using System.Text;
using Ceiralizer.Utils;

namespace Ceiralizer.Tests;

public class ChunkReaderTests
{
    private static string Hex(byte[] data) => $"[{string.Join(", ", data.Select(b => $"0x{b:X2}"))}]";

    #region ReadByte / ReadSByte

    [Theory]
    [InlineData(new byte[] { 0 }, 0)]
    [InlineData(new byte[] { 42 }, 42)]
    [InlineData(new byte[] { 255 }, 255)]
    public void ReadByte_ReturnsCorrectValue(byte[] data, byte expected)
    {
        var reader = new ChunkReader(data);
        Assert.Equal(expected, reader.ReadByte());
    }

    [Theory]
    [InlineData(new byte[] { 0 }, (sbyte)0)]
    [InlineData(new byte[] { 127 }, (sbyte)127)]
    [InlineData(new byte[] { 214 }, (sbyte)-42)]
    public void ReadSByte_ReturnsCorrectValue(byte[] data, sbyte expected)
    {
        var reader = new ChunkReader(data);
        Assert.Equal(expected, reader.ReadSByte());
    }

    [Fact]
    public void ReadByte_ThrowsWhenEmpty()
    {
        Assert.Throws<InvalidOperationException>(() => new ChunkReader([]).ReadByte());
    }

    #endregion

    #region ReadShort / ReadUShort

    [Theory]
    [InlineData(new byte[] { 0x00, 0x01 }, (short)256)]
    [InlineData(new byte[] { 0xFF, 0xFF }, (short)-1)]
    public void ReadShort_ReturnsCorrectValue(byte[] data, short expected)
    {
        Assert.Equal(expected, new ChunkReader(data).ReadShort());
    }

    [Theory]
    [InlineData(new byte[] { 0x00, 0x01 }, (ushort)256)]
    [InlineData(new byte[] { 0xFF, 0xFF }, (ushort)65535)]
    public void ReadUShort_ReturnsCorrectValue(byte[] data, ushort expected)
    {
        Assert.Equal(expected, new ChunkReader(data).ReadUShort());
    }

    #endregion

    #region ReadInt / ReadUInt

    [Theory]
    [InlineData(new byte[] { 0x78, 0x56, 0x34, 0x12 }, 0x12345678)]
    public void ReadInt_ReturnsCorrectValue(byte[] data, int expected)
    {
        Assert.Equal(expected, new ChunkReader(data).ReadInt());
    }

    [Fact]
    public void ReadInt_NegativeValue_ReturnsCorrectly()
    {
        Assert.Equal(-1000, new ChunkReader(BitConverter.GetBytes(-1000)).ReadInt());
    }

    [Fact]
    public void ReadUInt_MaxValue_ReturnsCorrectly()
    {
        Assert.Equal(uint.MaxValue, new ChunkReader(BitConverter.GetBytes(uint.MaxValue)).ReadUInt());
    }

    #endregion

    #region ReadLong / ReadULong

    [Theory]
    [InlineData(new byte[] { 0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01 }, 0x0123456789ABCDEF)]
    public void ReadLong_ReturnsCorrectValue(byte[] data, long expected)
    {
        Assert.Equal(expected, new ChunkReader(data).ReadLong());
    }

    [Fact]
    public void ReadLong_MinAndMax_ReturnsCorrectly()
    {
        var reader = new ChunkReader(BitConverter.GetBytes(long.MinValue).Concat(BitConverter.GetBytes(long.MaxValue)).ToArray());
        Assert.Equal(long.MinValue, reader.ReadLong());
        Assert.Equal(long.MaxValue, reader.ReadLong());
    }

    [Fact]
    public void ReadULong_MaxValue_ReturnsCorrectly()
    {
        Assert.Equal(ulong.MaxValue, new ChunkReader(BitConverter.GetBytes(ulong.MaxValue)).ReadULong());
    }

    #endregion

    #region ReadFloat / ReadDouble

    [Theory]
    [InlineData(3.14f)]
    [InlineData(0f)]
    [InlineData(-1.5f)]
    [InlineData(float.MaxValue)]
    [InlineData(float.MinValue)]
    public void ReadFloat_RoundTrip_ReturnsCorrectValue(float value)
    {
        Assert.Equal(value, new ChunkReader(BitConverter.GetBytes(value)).ReadFloat());
    }

    [Fact]
    public void ReadFloat_SpecialValues_ReturnsCorrectly()
    {
        var reader = new ChunkReader(BitConverter.GetBytes(float.NaN).Concat(BitConverter.GetBytes(float.PositiveInfinity)).Concat(BitConverter.GetBytes(float.NegativeInfinity)).ToArray());
        Assert.True(float.IsNaN(reader.ReadFloat()));
        Assert.True(float.IsPositiveInfinity(reader.ReadFloat()));
        Assert.True(float.IsNegativeInfinity(reader.ReadFloat()));
    }

    [Theory]
    [InlineData(2.718281828)]
    [InlineData(0.0)]
    [InlineData(-100.5)]
    [InlineData(double.MaxValue)]
    public void ReadDouble_RoundTrip_ReturnsCorrectValue(double value)
    {
        Assert.Equal(value, new ChunkReader(BitConverter.GetBytes(value)).ReadDouble());
    }

    [Fact]
    public void ReadDouble_SpecialValues_ReturnsCorrectly()
    {
        var reader = new ChunkReader(BitConverter.GetBytes(double.NaN).Concat(BitConverter.GetBytes(double.PositiveInfinity)).ToArray());
        Assert.True(double.IsNaN(reader.ReadDouble()));
        Assert.True(double.IsPositiveInfinity(reader.ReadDouble()));
    }

    #endregion

    #region ReadBool

    [Theory]
    [InlineData(new byte[] { 0 }, false)]
    [InlineData(new byte[] { 1 }, true)]
    [InlineData(new byte[] { 42 }, true)]
    [InlineData(new byte[] { 255 }, true)]
    public void ReadBool_ReturnsCorrectValue(byte[] data, bool expected)
    {
        Assert.Equal(expected, new ChunkReader(data).ReadBool());
    }

    #endregion

    #region ReadChar

    [Theory]
    [InlineData(new byte[] { 0x41 }, 'A')]
    [InlineData(new byte[] { 0xE2, 0x82, 0xAC }, '€')]
    public void ReadChar_Utf8_ReturnsCorrectValue(byte[] data, char expected)
    {
        Assert.Equal(expected, new ChunkReader(data).ReadChar(Encoding.UTF8));
    }

    [Fact]
    public void ReadChar_InsufficientData_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new ChunkReader([0xC2]).ReadChar(Encoding.UTF8));
    }

    [Fact]
    public void ReadChar_SurrogatePair_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new ChunkReader([0xF0, 0x9F, 0x9A, 0x80]).ReadChar(Encoding.UTF8));
    }

    #endregion

    #region ReadString

    [Theory]
    [InlineData("Hello")]
    [InlineData("")]
    [InlineData("Hello, 世界!")]
    [InlineData("Line1\nLine2\tTabbed")]
    public void ReadString_RoundTrip_ReturnsCorrectValue(string value)
    {
        var data = Encoding.UTF8.GetBytes(value);
        Assert.Equal(value, new ChunkReader(data).ReadString(Encoding.UTF8, data.Length));
    }

    #endregion

    #region ReadSegment

    [Fact]
    public void ReadSegment_ReturnsCorrectBytes()
    {
        var reader = new ChunkReader([1, 2, 3, 4, 5]);
        Assert.Equal([1, 2, 3], reader.ReadSegment(3).ToArray());
    }

    [Fact]
    public void ReadSegment_Zero_ReturnsEmpty()
    {
        Assert.Empty(new ChunkReader([1, 2, 3]).ReadSegment(0).ToArray());
    }

    [Fact]
    public void ReadSegment_AdvancesPosition()
    {
        var reader = new ChunkReader([1, 2, 3, 4, 5]);
        reader.ReadSegment(3);
        Assert.Equal(3, reader.Position);
    }

    [Fact]
    public void ReadSegment_NegativeLength_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChunkReader([1, 2, 3]).ReadSegment(-1));
    }

    #endregion

    #region Position / Remaining / CanRead

    [Fact]
    public void Position_TracksReads()
    {
        var reader = new ChunkReader([1, 2, 3, 4, 5]);
        Assert.Equal(0, reader.Position);
        reader.ReadByte();
        Assert.Equal(1, reader.Position);
        reader.ReadInt();
        Assert.Equal(5, reader.Position);
    }

    [Fact]
    public void Remaining_DecreasesWithReads()
    {
        var reader = new ChunkReader([1, 2, 3, 4, 5]);
        Assert.Equal(5, reader.Remaining);
        reader.ReadByte();
        Assert.Equal(4, reader.Remaining);
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    [InlineData(0, true)]
    public void CanRead_ReturnsCorrectValue(int length, bool expected)
    {
        Assert.Equal(expected, new ChunkReader([1, 2, 3, 4, 5]).CanRead(length));
    }

    #endregion

    #region ResetPosition

    [Fact]
    public void ResetPosition_ToZero_AllowsRereading()
    {
        var reader = new ChunkReader([42]);
        Assert.Equal(42, reader.ReadByte());
        reader.ResetPosition();
        Assert.Equal(42, reader.ReadByte());
    }

    [Fact]
    public void ResetPosition_ToSpecificPosition_SeeksCorrectly()
    {
        var reader = new ChunkReader([1, 2, 3, 4, 5]);
        reader.ReadByte();
        reader.ReadByte();
        reader.ResetPosition(1);
        Assert.Equal(1, reader.Position);
        Assert.Equal(2, reader.ReadByte());
    }

    [Fact]
    public void ResetPosition_OutOfBounds_AllowsSetting()
    {
        var reader = new ChunkReader([1, 2, 3]);
        reader.ResetPosition(10);
        Assert.Equal(10, reader.Position);
    }

    #endregion

    #region Constructors

    [Fact]
    public void FromByteArray_SameAsFromMemory()
    {
        byte[] data = [1, 2, 3, 4, 5];
        var fromArray = new ChunkReader(data);
        var fromMemory = new ChunkReader((ReadOnlyMemory<byte>)data);
        Assert.Equal(fromArray.ReadByte(), fromMemory.ReadByte());
        Assert.Equal(fromArray.Remaining, fromMemory.Remaining);
    }

    #endregion

    #region Error Cases

    [Fact]
    public void ReadBeyondData_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new ChunkReader([1, 2]).ReadInt());
    }

    [Fact]
    public void SequentialReads_CorrectPositionTracking()
    {
        var data = new[] { new byte[] { 1 }, BitConverter.GetBytes((short)1000), BitConverter.GetBytes(123456789), BitConverter.GetBytes(1234567890123456789L) }.SelectMany(x => x).ToArray();
        var reader = new ChunkReader(data);

        Assert.True(reader.ReadBool());
        Assert.Equal(1000, reader.ReadShort());
        Assert.Equal(123456789, reader.ReadInt());
        Assert.Equal(1234567890123456789L, reader.ReadLong());
        Assert.Equal(data.Length, reader.Position);
    }

    #endregion
}
