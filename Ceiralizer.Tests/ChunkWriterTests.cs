using System.Buffers;
using System.Text;
using Ceiralizer.Utils;

namespace Ceiralizer.Tests;

public class ChunkWriterTests
{
    private ChunkWriter CreateWriter() => new ChunkWriter(new ArrayBufferWriter<byte>());

    [Fact]
    public void WriteByte_SingleByte_WritesCorrectValue()
    {
        // Arrange
        var writer = CreateWriter();
        byte value = 42;

        // Act
        writer.Write(value);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Single(data);
        Assert.Equal(42, data[0]);
    }

    [Fact]
    public void WriteByte_MaxAndMinValues_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();

        // Act
        writer.Write(byte.MinValue);
        writer.Write(byte.MaxValue);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(2, data.Length);
        Assert.Equal(0, data[0]);
        Assert.Equal(255, data[1]);
    }

    [Fact]
    public void WriteSByte_SignedValues_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();

        // Act
        writer.Write((sbyte)-42);
        writer.Write((sbyte)127);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(2, data.Length);
        Assert.Equal(256 - 42, data[0]); // -42 in two's complement
        Assert.Equal(127, data[1]);
    }

    [Fact]
    public void WriteShort_LittleEndianOrder_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        short value = 256; // 0x0100

        // Act
        writer.Write(value);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(2, data.Length);
        Assert.Equal(0x00, data[0]); // Low byte
        Assert.Equal(0x01, data[1]); // High byte
    }

    [Fact]
    public void WriteUShort_LittleEndianOrder_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        ushort value = 256;

        // Act
        writer.Write(value);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(2, data.Length);
        Assert.Equal(0x00, data[0]);
        Assert.Equal(0x01, data[1]);
    }

    [Fact]
    public void WriteInt_LittleEndianOrder_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        int value = 0x12345678;

        // Act
        writer.Write(value);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(4, data.Length);
        Assert.Equal(0x78, data[0]);
        Assert.Equal(0x56, data[1]);
        Assert.Equal(0x34, data[2]);
        Assert.Equal(0x12, data[3]);
    }

    [Fact]
    public void WriteUInt_LittleEndianOrder_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        uint value = 0x12345678;

        // Act
        writer.Write(value);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(4, data.Length);
        Assert.Equal(0x78, data[0]);
        Assert.Equal(0x56, data[1]);
        Assert.Equal(0x34, data[2]);
        Assert.Equal(0x12, data[3]);
    }

    [Fact]
    public void WriteLong_LittleEndianOrder_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        long value = 0x0123456789ABCDEF;

        // Act
        writer.Write(value);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(8, data.Length);
        Assert.Equal(0xEF, data[0]);
        Assert.Equal(0xCD, data[1]);
        Assert.Equal(0xAB, data[2]);
        Assert.Equal(0x89, data[3]);
        Assert.Equal(0x67, data[4]);
        Assert.Equal(0x45, data[5]);
        Assert.Equal(0x23, data[6]);
        Assert.Equal(0x01, data[7]);
    }

    [Fact]
    public void WriteULong_LittleEndianOrder_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        ulong value = 0x0123456789ABCDEF;

        // Act
        writer.Write(value);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(8, data.Length);
        Assert.Equal(0xEF, data[0]);
        Assert.Equal(0xCD, data[1]);
        Assert.Equal(0xAB, data[2]);
        Assert.Equal(0x89, data[3]);
    }

    [Fact]
    public void WriteFloat_SpecificValue_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        float value = 3.14f;

        // Act
        writer.Write(value);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(4, data.Length);
        float result = BitConverter.Int32BitsToSingle(BitConverter.ToInt32(data, 0));
        Assert.Equal(value, result);
    }

    [Fact]
    public void WriteDouble_SpecificValue_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        double value = 2.718281828;

        // Act
        writer.Write(value);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(8, data.Length);
        double result = BitConverter.Int64BitsToDouble(BitConverter.ToInt64(data, 0));
        Assert.Equal(value, result);
    }

    [Fact]
    public void WriteFloat_ZeroAndNegative_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();

        // Act
        writer.Write(0f);
        writer.Write(-1.5f);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(8, data.Length);
    }

    [Fact]
    public void WriteBool_TrueValue_WritesOne()
    {
        // Arrange
        var writer = CreateWriter();

        // Act
        writer.Write(true);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Single(data);
        Assert.Equal(1, data[0]);
    }

    [Fact]
    public void WriteBool_FalseValue_WritesZero()
    {
        // Arrange
        var writer = CreateWriter();

        // Act
        writer.Write(false);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Single(data);
        Assert.Equal(0, data[0]);
    }

    [Fact]
    public void WriteBool_MultipleBools_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();

        // Act
        writer.Write(true);
        writer.Write(false);
        writer.Write(true);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(3, data.Length);
        Assert.Equal(1, data[0]);
        Assert.Equal(0, data[1]);
        Assert.Equal(1, data[2]);
    }

    [Fact]
    public void WriteString_SimpleAsciiString_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        string value = "Hello";

        // Act
        writer.Write(value, Encoding.UTF8);
        var data = writer.GetWrittenData();

        // Assert
        string result = Encoding.UTF8.GetString(data);
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void WriteString_EmptyString_WritesZeroBytes()
    {
        // Arrange
        var writer = CreateWriter();

        // Act
        writer.Write("", Encoding.UTF8);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Empty(data);
    }

    [Fact]
    public void WriteString_StringWithSpecialCharacters_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        string value = "Hello, 世界!";

        // Act
        writer.Write(value, Encoding.UTF8);
        var data = writer.GetWrittenData();

        // Assert
        string result = Encoding.UTF8.GetString(data);
        Assert.Equal(value, result);
    }

    [Fact]
    public void WriteString_DifferentEncodings_ProducesDifferentResults()
    {
        // Arrange
        var writer1 = CreateWriter();
        var writer2 = CreateWriter();
        string value = "A";

        // Act
        writer1.Write(value, Encoding.UTF8);
        writer2.Write(value, Encoding.ASCII);

        // Assert - they should be the same for ASCII character
        Assert.Equal(writer1.GetWrittenData(), writer2.GetWrittenData());
    }

    [Fact]
    public void WriteSpan_ByteSpan_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        ReadOnlySpan<byte> span = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        writer.Write(span);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, data);
    }

    [Fact]
    public void WriteSpan_EmptySpan_WritesNothing()
    {
        // Arrange
        var writer = CreateWriter();
        ReadOnlySpan<byte> span = ReadOnlySpan<byte>.Empty;

        // Act
        writer.Write(span);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Empty(data);
    }

    [Fact]
    public void MultipleWrites_SequentialValues_WritesInOrder()
    {
        // Arrange
        var writer = CreateWriter();

        // Act
        writer.Write((byte)1);
        writer.Write((short)256);
        writer.Write((int)65536);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(7, data.Length); // 1 + 2 + 4
        Assert.Equal(1, data[0]);
        Assert.Equal(0x00, data[1]);
        Assert.Equal(0x01, data[2]);
        Assert.Equal(0x00, data[3]);
        Assert.Equal(0x00, data[4]);
        Assert.Equal(0x01, data[5]);
        Assert.Equal(0x00, data[6]);
    }

    [Fact]
    public void GetWrittenData_ZeroWrites_ReturnsEmptyArray()
    {
        // Arrange
        var writer = CreateWriter();

        // Act
        var data = writer.GetWrittenData();

        // Assert
        Assert.Empty(data);
    }

    [Fact]
    public void Write_LargeNumber_WritesCorrectly()
    {
        // Arrange
        var writer = CreateWriter();
        int largeValue = 1000000;

        // Act
        writer.Write(largeValue);
        var data = writer.GetWrittenData();

        // Assert
        Assert.Equal(4, data.Length);
        int result = BitConverter.ToInt32(data, 0);
        Assert.Equal(largeValue, result);
    }
}

