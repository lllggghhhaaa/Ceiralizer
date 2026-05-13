using System.Text;
using Ceiralizer.Utils;

namespace Ceiralizer.Tests;

public class ChunkReaderTests
{
    [Fact]
    public void ReadByte_SingleByte_ReadsCorrectValue()
    {
        // Arrange
        var data = new byte[] { 42 };
        var reader = new ChunkReader(data);

        // Act
        byte result = reader.ReadByte();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void ReadByte_MaxAndMinValues_ReadsCorrectly()
    {
        // Arrange
        var data = new byte[] { byte.MinValue, byte.MaxValue };
        var reader = new ChunkReader(data);

        // Act
        byte min = reader.ReadByte();
        byte max = reader.ReadByte();

        // Assert
        Assert.Equal(0, min);
        Assert.Equal(255, max);
    }

    [Fact]
    public void ReadByte_ThrowsWhenNothingToRead()
    {
        // Arrange
        var reader = new ChunkReader(Array.Empty<byte>());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => reader.ReadByte());
    }

    [Fact]
    public void ReadByte_AdvancesPosition()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var reader = new ChunkReader(data);

        // Act
        Assert.Equal(0, reader.Position);
        reader.ReadByte();

        // Assert
        Assert.Equal(1, reader.Position);
    }

    [Fact]
    public void ReadSByte_SignedValues_ReadsCorrectly()
    {
        // Arrange
        var data = new byte[] { 256 - 42, 127 }; // -42 and 127 in two's complement
        var reader = new ChunkReader(data);

        // Act
        sbyte neg = reader.ReadSByte();
        sbyte pos = reader.ReadSByte();

        // Assert
        Assert.Equal(-42, neg);
        Assert.Equal(127, pos);
    }

    [Fact]
    public void ReadShort_LittleEndianOrder_ReadsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0x00, 0x01 }; // 256 in little-endian
        var reader = new ChunkReader(data);

        // Act
        short result = reader.ReadShort();

        // Assert
        Assert.Equal(256, result);
    }

    [Fact]
    public void ReadShort_NegativeValue_ReadsCorrectly()
    {
        // Arrange
        var data = BitConverter.GetBytes((short)-100);
        var reader = new ChunkReader(data);

        // Act
        short result = reader.ReadShort();

        // Assert
        Assert.Equal(-100, result);
    }

    [Fact]
    public void ReadUShort_LittleEndianOrder_ReadsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0x00, 0x01 };
        var reader = new ChunkReader(data);

        // Act
        ushort result = reader.ReadUShort();

        // Assert
        Assert.Equal((ushort)256, result);
    }

    [Fact]
    public void ReadInt_LittleEndianOrder_ReadsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0x78, 0x56, 0x34, 0x12 }; // 0x12345678
        var reader = new ChunkReader(data);

        // Act
        int result = reader.ReadInt();

        // Assert
        Assert.Equal(0x12345678, result);
    }

    [Fact]
    public void ReadInt_NegativeValue_ReadsCorrectly()
    {
        // Arrange
        var data = BitConverter.GetBytes(-1000);
        var reader = new ChunkReader(data);

        // Act
        int result = reader.ReadInt();

        // Assert
        Assert.Equal(-1000, result);
    }

    [Fact]
    public void ReadUInt_LittleEndianOrder_ReadsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0x78, 0x56, 0x34, 0x12 };
        var reader = new ChunkReader(data);

        // Act
        uint result = reader.ReadUInt();

        // Assert
        Assert.Equal(0x12345678U, result);
    }

    [Fact]
    public void ReadLong_LittleEndianOrder_ReadsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01 }; // 0x0123456789ABCDEF
        var reader = new ChunkReader(data);

        // Act
        long result = reader.ReadLong();

        // Assert
        Assert.Equal(0x0123456789ABCDEF, result);
    }

    [Fact]
    public void ReadULong_LittleEndianOrder_ReadsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01 };
        var reader = new ChunkReader(data);

        // Act
        ulong result = reader.ReadULong();

        // Assert
        Assert.Equal(0x0123456789ABCDEFU, result);
    }

    [Fact]
    public void ReadFloat_SpecificValue_ReadsCorrectly()
    {
        // Arrange
        float value = 3.14f;
        var data = BitConverter.GetBytes(value);
        var reader = new ChunkReader(data);

        // Act
        float result = reader.ReadFloat();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ReadFloat_ZeroAndNegative_ReadsCorrectly()
    {
        // Arrange
        var data = BitConverter.GetBytes(0f).Concat(BitConverter.GetBytes(-1.5f)).ToArray();
        var reader = new ChunkReader(data);

        // Act
        float zero = reader.ReadFloat();
        float neg = reader.ReadFloat();

        // Assert
        Assert.Equal(0f, zero);
        Assert.Equal(-1.5f, neg);
    }

    [Fact]
    public void ReadDouble_SpecificValue_ReadsCorrectly()
    {
        // Arrange
        double value = 2.718281828;
        var data = BitConverter.GetBytes(value);
        var reader = new ChunkReader(data);

        // Act
        double result = reader.ReadDouble();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ReadBool_TrueValue_ReadsTrue()
    {
        // Arrange
        var data = new byte[] { 1 };
        var reader = new ChunkReader(data);

        // Act
        bool result = reader.ReadBool();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ReadBool_FalseValue_ReadsFalse()
    {
        // Arrange
        var data = new byte[] { 0 };
        var reader = new ChunkReader(data);

        // Act
        bool result = reader.ReadBool();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ReadBool_AnyNonZeroValue_ReadsTrue()
    {
        // Arrange
        var data = new byte[] { 42 };
        var reader = new ChunkReader(data);

        // Act
        bool result = reader.ReadBool();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ReadChar_MultiByteUtf8Character_ReadsCorrectly()
    {
        // Arrange
        var data = new byte[] { 0xE2, 0x82, 0xAC }; // € symbol
        var reader = new ChunkReader(data);

        // Act
        char result = reader.ReadChar(Encoding.UTF8);

        // Assert
        Assert.Equal('€', result);
    }

    [Fact]
    public void ReadString_SimpleAsciiString_ReadsCorrectly()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("Hello");
        var reader = new ChunkReader(data);

        // Act
        string result = reader.ReadString(Encoding.UTF8, data.Length);

        // Assert
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ReadString_EmptyString_ReadsCorrectly()
    {
        // Arrange
        var data = Array.Empty<byte>();
        var reader = new ChunkReader(data);

        // Act
        string result = reader.ReadString(Encoding.UTF8, 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ReadString_StringWithSpecialCharacters_ReadsCorrectly()
    {
        // Arrange
        string original = "Hello, 世界!";
        var data = Encoding.UTF8.GetBytes(original);
        var reader = new ChunkReader(data);

        // Act
        string result = reader.ReadString(Encoding.UTF8, data.Length);

        // Assert
        Assert.Equal(original, result);
    }

    [Fact]
    public void ReadSegment_ValidRange_ReturnsCorrectSpan()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new ChunkReader(data);

        // Act
        var segment = reader.ReadSegment(3);

        // Assert
        Assert.Equal(new byte[] { 1, 2, 3 }, segment.ToArray());
    }

    [Fact]
    public void ReadSegment_ZeroLength_ReturnsEmptySpan()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var reader = new ChunkReader(data);

        // Act
        var segment = reader.ReadSegment(0);

        // Assert
        Assert.Empty(segment.ToArray());
    }

    [Fact]
    public void ReadSegment_AdvancesPosition()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new ChunkReader(data);

        // Act
        reader.ReadSegment(3);

        // Assert
        Assert.Equal(3, reader.Position);
    }

    [Fact]
    public void ThrowsWhenReadingBeyondData()
    {
        // Arrange
        var data = new byte[] { 1, 2 };
        var reader = new ChunkReader(data);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => reader.ReadInt());
    }

    [Fact]
    public void CanRead_WithEnoughData_ReturnsTrue()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new ChunkReader(data);

        // Act
        bool can = reader.CanRead(3);

        // Assert
        Assert.True(can);
    }

    [Fact]
    public void CanRead_WithNotEnoughData_ReturnsFalse()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var reader = new ChunkReader(data);

        // Act
        bool can = reader.CanRead(5);

        // Assert
        Assert.False(can);
    }

    [Fact]
    public void Remaining_ReturnsCorrectValue()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new ChunkReader(data);

        // Act & Assert
        Assert.Equal(5, reader.Remaining);
        reader.ReadByte();
        Assert.Equal(4, reader.Remaining);
        reader.ReadByte();
        Assert.Equal(3, reader.Remaining);
    }

    [Fact]
    public void Position_TracksCurrentReadPosition()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new ChunkReader(data);

        // Act & Assert
        Assert.Equal(0, reader.Position);
        reader.ReadByte();
        Assert.Equal(1, reader.Position);
        reader.ReadInt(); // 4 bytes
        Assert.Equal(5, reader.Position);
    }

    [Fact]
    public void ResetPosition_ResetsToZero_AllowsRereadingData()
    {
        // Arrange
        var data = new byte[] { 42 };
        var reader = new ChunkReader(data);

        // Act
        byte first = reader.ReadByte();
        reader.ResetPosition();
        byte second = reader.ReadByte();

        // Assert
        Assert.Equal(first, second);
        Assert.Equal(42, second);
    }

    [Fact]
    public void ResetPosition_ResetsToSpecificPosition()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var reader = new ChunkReader(data);
        reader.ReadByte();
        reader.ReadByte();

        // Act
        reader.ResetPosition(1);

        // Assert
        Assert.Equal(1, reader.Position);
        byte value = reader.ReadByte();
        Assert.Equal(2, value);
    }

    [Fact]
    public void MultipleReads_SequentialValues_ReadsInOrder()
    {
        // Arrange
        byte b = 1;
        short s = 256;
        int i = 65536;
        var data = new byte[7];
        Array.Copy(new byte[] { b }, 0, data, 0, 1);
        Array.Copy(BitConverter.GetBytes(s), 0, data, 1, 2);
        Array.Copy(BitConverter.GetBytes(i), 0, data, 3, 4);
        var reader = new ChunkReader(data);

        // Act
        byte rb = reader.ReadByte();
        short rs = reader.ReadShort();
        int ri = reader.ReadInt();

        // Assert
        Assert.Equal(b, rb);
        Assert.Equal(s, rs);
        Assert.Equal(i, ri);
    }

    [Fact]
    public void ReadFromMemory_Works()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var memory = new ReadOnlyMemory<byte>(data);
        var reader = new ChunkReader(memory);

        // Act
        byte first = reader.ReadByte();
        byte second = reader.ReadByte();

        // Assert
        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }

    [Fact]
    public void ReadNegativeLength_ThrowsException()
    {
        // Arrange
        var reader = new ChunkReader(new byte[] { 1, 2, 3 });

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadSegment(-1));
    }
}

