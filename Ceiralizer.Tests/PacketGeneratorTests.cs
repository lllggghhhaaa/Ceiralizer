namespace Ceiralizer.Tests;

public class PacketGeneratorTests(ITestOutputHelper output)
{
    private static string Hex(byte[] data) => $"[{string.Join(", ", data.Select(b => $"0x{b:X2}"))}] ({data.Length} bytes)";

    #region Simple Packet

    [Theory]
    [InlineData(42, "Hello, world!")]
    [InlineData(0, "Zero")]
    [InlineData(-999, "Negative")]
    [InlineData(int.MaxValue, "Max")]
    [InlineData(int.MinValue, "Min")]
    public void SimplePacket_RoundTrip_PreservesValues(int id, string name)
    {
        var original = new SimplePacket { Id = id, Name = name };
        var result = SimplePacket.Deserialize(original.Serialize());

        output.WriteLine($"Id: {original.Id} -> {result.Id}");
        output.WriteLine($"Name: '{original.Name}' -> '{result.Name}'");

        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.Name, result.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Test\n\t\r\"'")]
    [InlineData("你好世界🚀")]
    public void SimplePacket_NameRoundTrip_PreservesSpecialCharacters(string name)
    {
        var original = new SimplePacket { Id = 1, Name = name };
        var result = SimplePacket.Deserialize(original.Serialize());
        Assert.Equal(name, result.Name);
    }

    [Fact]
    public void SimplePacket_Serialize_ProducesExpectedBinaryLayout()
    {
        var packet = new SimplePacket { Id = 256, Name = "AB" };
        byte[] expected = [0x00, 0x01, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x41, 0x42];
        Assert.Equal(expected, packet.Serialize());
    }

    #endregion

    #region Array Packet

    [Theory]
    [InlineData(new[] { 1, 2, 3, 4 }, new[] { "Alice", "Bob" })]
    [InlineData(new int[] { }, new string[] { })]
    [InlineData(new[] { -1, -100, int.MinValue }, new[] { "Neg" })]
    public void PacketWithArray_RoundTrip_PreservesValues(int[] values, string[] names)
    {
        var original = new PacketWithArray { Values = values, Names = names };
        var result = PacketWithArray.Deserialize(original.Serialize());

        Assert.Equal(original.Values, result.Values);
        Assert.Equal(original.Names, result.Names);
    }

    [Fact]
    public void PacketWithArray_LargeArray_RoundTrip()
    {
        var values = Enumerable.Range(1, 1000).ToArray();
        var original = new PacketWithArray { Values = values, Names = ["Test"] };
        var result = PacketWithArray.Deserialize(original.Serialize());
        Assert.Equal(values, result.Values);
    }

    [Fact]
    public void PacketWithArray_EmptyStrings_RoundTrip()
    {
        var original = new PacketWithArray { Values = [1], Names = ["", "NonEmpty", ""] };
        var result = PacketWithArray.Deserialize(original.Serialize());
        Assert.Equal(["", "NonEmpty", ""], result.Names);
    }

    [Fact]
    public void PacketWithArray_Serialize_ProducesExpectedBinaryLayout()
    {
        var packet = new PacketWithArray { Values = [1, 2], Names = ["X", "YZ"] };
        byte[] expected = [0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x58, 0x02, 0x00, 0x00, 0x00, 0x59, 0x5A];
        Assert.Equal(expected, packet.Serialize());
    }

    #endregion

    #region Nested Packet

    [Theory]
    [InlineData(7, 1.5f, -3.14f)]
    [InlineData(0, 0f, 0f)]
    [InlineData(1, float.MaxValue, float.MinValue)]
    public void OuterPacket_RoundTrip_PreservesValues(int count, float x, float y)
    {
        var original = new OuterPacket { Count = count, Position = new InnerPacket { X = x, Y = y } };
        var result = OuterPacket.Deserialize(original.Serialize());

        Assert.Equal(original.Count, result.Count);
        Assert.Equal(original.Position.X, result.Position.X);
        Assert.Equal(original.Position.Y, result.Position.Y);
    }

    #endregion

    #region Custom Serializer / ISerializable

    [Theory]
    [InlineData(99, "Test")]
    [InlineData(42, "")]
    [InlineData(0, "Zero")]
    public void PacketWithSerializable_RoundTrip_PreservesValues(int a, string b)
    {
        var original = new PacketWithSerializable { Data = new FooBar(a, b) };
        var result = PacketWithSerializable.Deserialize(original.Serialize());
        Assert.Equal(original.Data, result.Data);
    }

    [Theory]
    [InlineData(1.0f, 2.0f, 3.0f)]
    [InlineData(0f, 0f, 0f)]
    [InlineData(-1.5f, -2.5f, -3.5f)]
    public void PacketWithCustomSerializer_RoundTrip_PreservesValues(float x, float y, float z)
    {
        var original = new PacketWithCustomSerializer { Position = new Vector3(x, y, z) };
        var result = PacketWithCustomSerializer.Deserialize(original.Serialize());
        Assert.Equal(original.Position, result.Position);
    }

    #endregion

    #region Complex Packet

    [Fact]
    public void ComplexPacket_AllTypes_RoundTrip()
    {
        var original = new ComplexPacket
        {
            Flags = 0xAB,
            Symbol = '$',
            Inner = new InnerPacket { X = 10.0f, Y = 20.0f },
            SerializableData = new FooBar(5, "Foo"),
            Path = [new Vector3(0, 0, 0), new Vector3(1, 1, 1)]
        };
        var result = ComplexPacket.Deserialize(original.Serialize());

        Assert.Equal(original.Flags, result.Flags);
        Assert.Equal(original.Symbol, result.Symbol);
        Assert.Equal(original.Inner.X, result.Inner.X);
        Assert.Equal(original.Inner.Y, result.Inner.Y);
        Assert.Equal(original.SerializableData, result.SerializableData);
        Assert.Equal(original.Path, result.Path);
    }

    [Fact]
    public void ComplexPacket_LargePath_RoundTrip()
    {
        var path = Enumerable.Range(0, 100).Select(i => new Vector3(i, i * 2, i * 3)).ToArray();
        var original = new ComplexPacket { Flags = 255, Symbol = 'Z', Inner = new InnerPacket { X = -100f, Y = 200f }, SerializableData = new FooBar(int.MaxValue, "Large"), Path = path };
        var result = ComplexPacket.Deserialize(original.Serialize());
        Assert.Equal(100, result.Path.Length);
        Assert.Equal(original.Path, result.Path);
    }

    [Fact]
    public void ComplexPacket_EmptyPath_RoundTrip()
    {
        var original = new ComplexPacket { Flags = 0, Symbol = 'A', Inner = new InnerPacket { X = 0f, Y = 0f }, SerializableData = new FooBar(0, ""), Path = [] };
        var result = ComplexPacket.Deserialize(original.Serialize());
        Assert.Empty(result.Path);
    }

    [Fact]
    public void ComplexPacket_UnicodeSymbol_RoundTrip()
    {
        var original = new ComplexPacket { Flags = 1, Symbol = '€', Inner = new InnerPacket { X = 1f, Y = 1f }, SerializableData = new FooBar(1, "U"), Path = [new Vector3(1, 1, 1)] };
        var result = ComplexPacket.Deserialize(original.Serialize());
        Assert.Equal('€', result.Symbol);
    }

    #endregion

    #region Character Encoding

    [Theory]
    [InlineData('A')]
    [InlineData('日')]
    [InlineData('€')]
    public void CharOnlyPacket_RoundTrip_PreservesValue(char ch)
    {
        var original = new CharOnlyPacket { Ch = ch };
        var result = CharOnlyPacket.Deserialize(original.Serialize());
        Assert.Equal(ch, result.Ch);
    }

    #endregion

    #region Roundtrip Consistency

    [Fact]
    public void MultipleRoundTrips_ProduceConsistentResults()
    {
        var original = new SimplePacket { Id = 123, Name = "Test" };
        var first = SimplePacket.Deserialize(original.Serialize());
        var second = SimplePacket.Deserialize(first.Serialize());
        var third = SimplePacket.Deserialize(second.Serialize());

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
    public void SimplePacket_DifferentIds_RoundTrip(int id)
    {
        var result = SimplePacket.Deserialize(new SimplePacket { Id = id, Name = "Test" }.Serialize());
        Assert.Equal(id, result.Id);
    }

    #endregion
}
