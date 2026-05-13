namespace Ceiralizer.Tests;

/// <summary>
/// Tests for packet serialization and deserialization functionality.
/// Covers various data types, nested structures, arrays, and custom serializers.
/// </summary>
public class PacketGeneratorTests
{
    #region Simple Packet Tests

    [Fact]
    public void SimplePacket_RoundTrip_ProducesEqualValues()
    {
        // Arrange
        var original = new SimplePacket { Id = 42, Name = "Hello, world!" };

        // Act
        var result = SimplePacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.Name, result.Name);
    }

    [Fact]
    public void SimplePacket_WithZeroId_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new SimplePacket { Id = 0, Name = "Zero" };

        // Act
        var result = SimplePacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(0, result.Id);
        Assert.Equal("Zero", result.Name);
    }

    [Fact]
    public void SimplePacket_WithNegativeId_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new SimplePacket { Id = -999, Name = "Negative" };

        // Act
        var result = SimplePacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(-999, result.Id);
    }

    [Fact]
    public void SimplePacket_WithMaxIntId_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new SimplePacket { Id = int.MaxValue, Name = "Max" };

        // Act
        var result = SimplePacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(int.MaxValue, result.Id);
    }

    [Fact]
    public void SimplePacket_WithEmptyName_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new SimplePacket { Id = 1, Name = "" };

        // Act
        var result = SimplePacket.Deserialize(original.Serialize());

        // Assert
        Assert.Empty(result.Name);
    }

    [Fact]
    public void SimplePacket_WithSpecialCharacters_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new SimplePacket { Id = 1, Name = "Test\n\t\r\"'" };

        // Act
        var result = SimplePacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal("Test\n\t\r\"'", result.Name);
    }

    [Fact]
    public void SimplePacket_WithUnicodeCharacters_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new SimplePacket { Id = 1, Name = "你好世界🚀" };

        // Act
        var result = SimplePacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal("你好世界🚀", result.Name);
    }

    [Fact]
    public void Serialize_SimplePacket_ProducesExpectedBinaryLayout()
    {
        // Arrange
        var packet = new SimplePacket { Id = 256, Name = "AB" };
        byte[] expectedBytes =
        [
            0x00, 0x01, 0x00, 0x00, // Id (int, little-endian)
            0x02, 0x00, 0x00, 0x00, // Name length (int)
            0x41, 0x42 // "AB" UTF-8 bytes
        ];

        // Act
        byte[] actual = packet.Serialize();

        // Assert
        Assert.Equal(expectedBytes, actual);
    }

    #endregion

    #region Array Packet Tests

    [Fact]
    public void PacketWithArray_RoundTrip_ProducesEqualElements()
    {
        // Arrange
        var original = new PacketWithArray
        {
            Values = [1, 2, 3, 4],
            Names = ["Alice", "Bob"]
        };

        // Act
        var result = PacketWithArray.Deserialize(original.Serialize());

        // Assert
        Assert.True(original.Values.SequenceEqual(result.Values));
        Assert.True(original.Names.SequenceEqual(result.Names));
    }

    [Fact]
    public void PacketWithArray_RoundTrip_ProducesEqualElements_LargeArray()
    {
        // Arrange
        var values = Enumerable.Range(1, 1000).ToArray();
        var original = new PacketWithArray
        {
            Values = values,
            Names = ["Test"]
        };

        // Act
        var result = PacketWithArray.Deserialize(original.Serialize());

        // Assert
        Assert.True(original.Values.SequenceEqual(result.Values));
    }

    [Fact]
    public void PacketWithEmptyArrays_RoundTrip_ReturnsEmptyArrays()
    {
        // Arrange
        var original = new PacketWithArray
        {
            Values = Array.Empty<int>(),
            Names = Array.Empty<string>()
        };

        // Act
        var result = PacketWithArray.Deserialize(original.Serialize());

        // Assert
        Assert.Empty(result.Values);
        Assert.Empty(result.Names);
    }

    [Fact]
    public void PacketWithArray_WithNegativeValues_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new PacketWithArray
        {
            Values = [-1, -100, int.MinValue],
            Names = ["Neg"]
        };

        // Act
        var result = PacketWithArray.Deserialize(original.Serialize());

        // Assert
        Assert.Equal([-1, -100, int.MinValue], result.Values);
    }

    [Fact]
    public void PacketWithArray_WithEmptyStrings_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new PacketWithArray
        {
            Values = [1],
            Names = ["", "NonEmpty", ""]
        };

        // Act
        var result = PacketWithArray.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(["", "NonEmpty", ""], result.Names);
    }

    [Fact]
    public void Serialize_PacketWithArray_ProducesExpectedBinaryLayout()
    {
        // Arrange
        var packet = new PacketWithArray
        {
            Values = [1, 2],
            Names = ["X", "YZ"]
        };
        byte[] expectedBytes =
        {
            0x02, 0x00, 0x00, 0x00, // Values length
            0x01, 0x00, 0x00, 0x00, // Values[0] = 1
            0x02, 0x00, 0x00, 0x00, // Values[1] = 2
            0x02, 0x00, 0x00, 0x00, // Names length
            0x01, 0x00, 0x00, 0x00, 0x58, // "X"
            0x02, 0x00, 0x00, 0x00, 0x59, 0x5A // "YZ"
        };

        // Act
        byte[] actual = packet.Serialize();

        // Assert
        Assert.Equal(expectedBytes, actual);
    }

    #endregion

    #region Nested Packet Tests

    [Fact]
    public void OuterPacket_WithNestedPacket_RoundTrip_PreservesAllFields()
    {
        // Arrange
        var original = new OuterPacket
        {
            Count = 7,
            Position = new InnerPacket { X = 1.5f, Y = -3.14f }
        };

        // Act
        var result = OuterPacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(original.Count, result.Count);
        Assert.Equal(original.Position.X, result.Position.X);
        Assert.Equal(original.Position.Y, result.Position.Y);
    }

    [Fact]
    public void OuterPacket_WithZeroCount_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new OuterPacket
        {
            Count = 0,
            Position = new InnerPacket { X = 0f, Y = 0f }
        };

        // Act
        var result = OuterPacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(0, result.Count);
        Assert.Equal(0f, result.Position.X);
        Assert.Equal(0f, result.Position.Y);
    }

    [Fact]
    public void InnerPacket_WithFloatEdgeCases_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new OuterPacket
        {
            Count = 1,
            Position = new InnerPacket { X = float.MaxValue, Y = float.MinValue }
        };

        // Act
        var result = OuterPacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(original.Position.X, result.Position.X);
        Assert.Equal(original.Position.Y, result.Position.Y);
    }

    #endregion

    #region Custom Serializer Tests

    [Fact]
    public void PacketWithSerializable_RoundTrip_EqualsOriginalData()
    {
        // Arrange
        var original = new PacketWithSerializable
        {
            Data = new FooBar(99, "Test")
        };

        // Act
        var result = PacketWithSerializable.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(original.Data, result.Data);
    }

    [Fact]
    public void PacketWithSerializable_WithEmptyString_RoundTrip_PreservesData()
    {
        // Arrange
        var original = new PacketWithSerializable
        {
            Data = new FooBar(42, "")
        };

        // Act
        var result = PacketWithSerializable.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(42, result.Data.A);
        Assert.Empty(result.Data.B);
    }

    [Fact]
    public void PacketWithSerializable_WithZeroValue_RoundTrip_PreservesData()
    {
        // Arrange
        var original = new PacketWithSerializable
        {
            Data = new FooBar(0, "Zero")
        };

        // Act
        var result = PacketWithSerializable.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(0, result.Data.A);
        Assert.Equal("Zero", result.Data.B);
    }

    [Fact]
    public void PacketWithCustomSerializer_RoundTrip_EqualsOriginalPosition()
    {
        // Arrange
        var original = new PacketWithCustomSerializer
        {
            Position = new Vector3(1.0f, 2.0f, 3.0f)
        };

        // Act
        var result = PacketWithCustomSerializer.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(original.Position, result.Position);
    }

    [Fact]
    public void PacketWithCustomSerializer_WithZeroVector_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new PacketWithCustomSerializer
        {
            Position = new Vector3(0f, 0f, 0f)
        };

        // Act
        var result = PacketWithCustomSerializer.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(Vector3.Zero, result.Position);
    }

    [Fact]
    public void PacketWithCustomSerializer_WithNegativeValues_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new PacketWithCustomSerializer
        {
            Position = new Vector3(-1.5f, -2.5f, -3.5f)
        };

        // Act
        var result = PacketWithCustomSerializer.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(original.Position, result.Position);
    }

    #endregion

    #region Complex Packet Tests

    [Fact]
    public void ComplexPacket_WithAllSupportedTypes_RoundTrip_PreservesAllValues()
    {
        // Arrange
        var original = new ComplexPacket
        {
            Flags = 0xAB,
            Symbol = '$',
            Inner = new InnerPacket { X = 10.0f, Y = 20.0f },
            SerializableData = new FooBar(5, "Foo"),
            Path =
            [
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 1)
            ]
        };

        // Act
        var result = ComplexPacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(original.Flags, result.Flags);
        Assert.Equal(original.Symbol, result.Symbol);
        Assert.Equal(original.Inner.X, result.Inner.X);
        Assert.Equal(original.Inner.Y, result.Inner.Y);
        Assert.Equal(original.SerializableData, result.SerializableData);
        Assert.True(original.Path.SequenceEqual(result.Path));
    }

    [Fact]
    public void ComplexPacket_WithLargePath_RoundTrip_PreservesAllValues()
    {
        // Arrange
        var path = Enumerable.Range(0, 100)
            .Select(i => new Vector3(i, i * 2, i * 3))
            .ToArray();
        var original = new ComplexPacket
        {
            Flags = 255,
            Symbol = 'Z',
            Inner = new InnerPacket { X = -100f, Y = 200f },
            SerializableData = new FooBar(int.MaxValue, "Large"),
            Path = path
        };

        // Act
        var result = ComplexPacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(100, result.Path.Length);
        Assert.True(original.Path.SequenceEqual(result.Path));
    }

    [Fact]
    public void ComplexPacket_WithEmptyPath_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new ComplexPacket
        {
            Flags = 0,
            Symbol = 'A',
            Inner = new InnerPacket { X = 0f, Y = 0f },
            SerializableData = new FooBar(0, ""),
            Path = Array.Empty<Vector3>()
        };

        // Act
        var result = ComplexPacket.Deserialize(original.Serialize());

        // Assert
        Assert.Empty(result.Path);
    }

    [Fact]
    public void ComplexPacket_WithUnicodeSymbol_RoundTrip_PreservesValue()
    {
        // Arrange
        var original = new ComplexPacket
        {
            Flags = 1,
            Symbol = '€',
            Inner = new InnerPacket { X = 1f, Y = 1f },
            SerializableData = new FooBar(1, "U"),
            Path = [new Vector3(1, 1, 1)]
        };

        // Act
        var result = ComplexPacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal('€', result.Symbol);
    }

    #endregion

    #region Character Encoding Tests

    [Fact]
    public void CharOnlyPacket_WithAsciiCharacter_RoundTrip_PreservesValue()
    {
        // Arrange
        var original = new CharOnlyPacket { Ch = 'A' };

        // Act
        var result = CharOnlyPacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal('A', result.Ch);
    }

    [Fact]
    public void CharOnlyPacket_WithUnicodeCharacter_RoundTrip_PreservesValue()
    {
        // Arrange
        var original = new CharOnlyPacket { Ch = '日' }; // Japanese character
        
        // Act
        var result = CharOnlyPacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal('日', result.Ch);
    }

    #endregion

    #region Roundtrip Consistency Tests

    [Fact]
    public void MultipleRoundTrips_ProduceConsistentResults()
    {
        // Arrange
        var original = new SimplePacket { Id = 123, Name = "Test" };

        // Act
        var first = SimplePacket.Deserialize(original.Serialize());
        var second = SimplePacket.Deserialize(first.Serialize());
        var third = SimplePacket.Deserialize(second.Serialize());

        // Assert
        Assert.Equal(original.Id, first.Id);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(second.Id, third.Id);
        Assert.Equal(original.Name, third.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void SimplePacket_WithDifferentIds_RoundTrip_PreservesId(int id)
    {
        // Arrange
        var original = new SimplePacket { Id = id, Name = "Test" };

        // Act
        var result = SimplePacket.Deserialize(original.Serialize());

        // Assert
        Assert.Equal(id, result.Id);
    }

    #endregion
}