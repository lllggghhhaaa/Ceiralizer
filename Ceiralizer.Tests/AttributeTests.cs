using Ceiralizer.Models;

namespace Ceiralizer.Tests;

public class AttributeTests(ITestOutputHelper output)
{
    private static string Hex(byte[] data) => $"[{string.Join(", ", data.Select(b => $"0x{b:X2}"))}] ({data.Length} bytes)";

    #region Field Order

    [Fact]
    public void OrderedPacket_Serialize_InSpecifiedOrder()
    {
        var packet = new OrderedPacket { First = 0x11111111, Second = 0x22222222, Third = 0x33333333 };
        var data = packet.Serialize();

        output.WriteLine($"Data: {Hex(data)}");
        Assert.Equal(12, data.Length);
        Assert.Equal(0x11111111, BitConverter.ToInt32(data, 0));
        Assert.Equal(0x22222222, BitConverter.ToInt32(data, 4));
        Assert.Equal(0x33333333, BitConverter.ToInt32(data, 8));
    }

    [Theory]
    [InlineData(10, 20, 30)]
    [InlineData(-1, -100, int.MinValue)]
    [InlineData(int.MaxValue, 0, int.MinValue)]
    public void OrderedPacket_RoundTrip_PreservesValues(int first, int second, int third)
    {
        var original = new OrderedPacket { First = first, Second = second, Third = third };
        var result = OrderedPacket.Deserialize(original.Serialize());
        Assert.Equal(original.First, result.First);
        Assert.Equal(original.Second, result.Second);
        Assert.Equal(original.Third, result.Third);
    }

    [Fact]
    public void DuplicateOrder_RoundTrip_PreservesValues()
    {
        var original = new DuplicateOrderPacket { First = 1, Second = 2, Third = 3 };
        var result = DuplicateOrderPacket.Deserialize(original.Serialize());
        Assert.Equal(original.First, result.First);
        Assert.Equal(original.Second, result.Second);
        Assert.Equal(original.Third, result.Third);
    }

    [Fact]
    public void NegativeOrder_Serialize_InCorrectOrder()
    {
        var original = new NegativeOrderPacket { First = 1, Second = 2, Third = 3 };
        var data = original.Serialize();
        Assert.Equal(2, BitConverter.ToInt32(data, 0));
        Assert.Equal(1, BitConverter.ToInt32(data, 4));
        Assert.Equal(3, BitConverter.ToInt32(data, 8));
    }

    #endregion

    #region String Options

    [Fact]
    public void StringOptionsPacket_AllFields_RoundTrip()
    {
        var original = new StringOptionsPacket { AsciiShort = "Hi", UnicodeString = "\u4E16\u754C", BytePrefixString = "Byte!", DefaultString = "Default" };
        var result = StringOptionsPacket.Deserialize(original.Serialize());

        output.WriteLine($"Bytes: {Hex(original.Serialize())}");
        Assert.Equal(original.AsciiShort, result.AsciiShort);
        Assert.Equal(original.UnicodeString, result.UnicodeString);
        Assert.Equal(original.BytePrefixString, result.BytePrefixString);
        Assert.Equal(original.DefaultString, result.DefaultString);
    }

    [Fact]
    public void StringOptionsPacket_EmptyStrings_RoundTrip()
    {
        var original = new StringOptionsPacket { AsciiShort = "", UnicodeString = "", BytePrefixString = "", DefaultString = "" };
        var result = StringOptionsPacket.Deserialize(original.Serialize());
        Assert.All(new[] { result.AsciiShort, result.UnicodeString, result.BytePrefixString, result.DefaultString }, s => Assert.Empty(s));
    }

    [Fact]
    public void AsciiEncoding_ReplacesNonAscii()
    {
        var result = StringOptionsPacket.Deserialize(new StringOptionsPacket { AsciiShort = "caf\u00E9", UnicodeString = "", BytePrefixString = "", DefaultString = "" }.Serialize());
        Assert.Equal("caf?", result.AsciiShort);
    }

    [Fact]
    public void AsciiEncoding_PreservesAscii()
    {
        var original = new StringOptionsPacket { AsciiShort = "Hello World! 123", UnicodeString = "", BytePrefixString = "", DefaultString = "" };
        Assert.Equal(original.AsciiShort, StringOptionsPacket.Deserialize(original.Serialize()).AsciiShort);
    }

    [Fact]
    public void UnicodeEncoding_PreservesMultilingual()
    {
        var original = new StringOptionsPacket { AsciiShort = "", UnicodeString = "Hello \u4E16\u754C \u00E9\u00E0\u00FC", BytePrefixString = "", DefaultString = "" };
        Assert.Equal(original.UnicodeString, StringOptionsPacket.Deserialize(original.Serialize()).UnicodeString);
    }

    [Fact]
    public void BytePrefixString_LongString_RoundTrip()
    {
        var original = new StringOptionsPacket { AsciiShort = "", UnicodeString = "", BytePrefixString = new string('A', 200), DefaultString = "" };
        var result = StringOptionsPacket.Deserialize(original.Serialize());
        Assert.Equal(200, result.BytePrefixString.Length);
        Assert.Equal(original.BytePrefixString, result.BytePrefixString);
    }

    [Fact]
    public void DefaultString_SpecialCharacters_RoundTrip()
    {
        var original = new StringOptionsPacket { AsciiShort = "", UnicodeString = "", BytePrefixString = "", DefaultString = "Line1\nLine2\tTabbed\"Quoted" };
        Assert.Equal(original.DefaultString, StringOptionsPacket.Deserialize(original.Serialize()).DefaultString);
    }

    [Fact]
    public void ShortPrefix_TwoByteLength()
    {
        var data = new StringOptionsPacket { AsciiShort = "ABC", UnicodeString = "", BytePrefixString = "", DefaultString = "" }.Serialize();
        Assert.Equal(0x03, data[0]);
        Assert.Equal(0x00, data[1]);
        Assert.Equal(0x41, data[2]);
    }

    [Fact]
    public void BytePrefix_OneByteLength()
    {
        var data = new StringOptionsPacket { AsciiShort = "", UnicodeString = "", BytePrefixString = "AB", DefaultString = "" }.Serialize();
        Assert.Equal(0x02, data[6]);
        Assert.Equal(0x41, data[7]);
    }

    [Fact]
    public void DifferentEncodings_DifferentByteCounts()
    {
        var asciiLen = new StringOptionsPacket { AsciiShort = "\u00E9", UnicodeString = "", BytePrefixString = "", DefaultString = "" }.Serialize().Length;
        var unicodeLen = new StringOptionsPacket { AsciiShort = "", UnicodeString = "\u00E9", BytePrefixString = "", DefaultString = "" }.Serialize().Length;
        Assert.NotEqual(asciiLen, unicodeLen);
    }

    #endregion

    #region Additional Encodings

    [Theory]
    [InlineData("Hello World")]
    [InlineData("Test 123")]
    public void Utf7Encoding_RoundTrip(string value)
    {
        var original = new Utf7StringPacket { Utf7String = value };
        Assert.Equal(value, Utf7StringPacket.Deserialize(original.Serialize()).Utf7String);
    }

    [Theory]
    [InlineData("Hello \u4E16\u754C")]
    [InlineData("Test")]
    public void Utf32Encoding_RoundTrip(string value)
    {
        var original = new Utf32StringPacket { Utf32String = value };
        Assert.Equal(value, Utf32StringPacket.Deserialize(original.Serialize()).Utf32String);
    }

    [Theory]
    [InlineData("Test \u00E9\u00E0")]
    [InlineData("Hello")]
    public void BigEndianEncoding_RoundTrip(string value)
    {
        var original = new BigEndianStringPacket { BigEndianString = value };
        Assert.Equal(value, BigEndianStringPacket.Deserialize(original.Serialize()).BigEndianString);
    }

    [Fact]
    public void DifferentEncodings_DifferentSizes()
    {
        var utf7Len = new Utf7StringPacket { Utf7String = "ABC" }.Serialize().Length;
        var utf32Len = new Utf32StringPacket { Utf32String = "ABC" }.Serialize().Length;
        var beLen = new BigEndianStringPacket { BigEndianString = "ABC" }.Serialize().Length;
        output.WriteLine($"UTF7: {utf7Len}, UTF32: {utf32Len}, BigEndian: {beLen}");
        Assert.NotEqual(utf7Len, utf32Len);
        Assert.NotEqual(utf32Len, beLen);
    }

    #endregion

    #region Additional Prefix Types

    [Theory]
    [InlineData("Test")]
    [InlineData("A")]
    [InlineData("")]
    public void SBytePrefix_RoundTrip(string value)
    {
        var original = new SBytePrefixPacket { SBytePrefixed = value };
        var result = SBytePrefixPacket.Deserialize(original.Serialize());
        Assert.Equal(value, result.SBytePrefixed);
    }

    [Fact]
    public void SBytePrefix_OneByteLength()
    {
        var data = new SBytePrefixPacket { SBytePrefixed = "AB" }.Serialize();
        Assert.Equal([0x02, 0x41, 0x42], data);
    }

    [Theory]
    [InlineData("Hello World")]
    [InlineData("")]
    public void ShortPrefix_RoundTrip(string value)
    {
        var original = new ShortPrefixPacket { ShortPrefixed = value };
        Assert.Equal(value, ShortPrefixPacket.Deserialize(original.Serialize()).ShortPrefixed);
    }

    [Theory]
    [InlineData("Test String")]
    [InlineData("")]
    public void UShortPrefix_RoundTrip(string value)
    {
        var original = new UShortPrefixPacket { UShortPrefixed = value };
        Assert.Equal(value, UShortPrefixPacket.Deserialize(original.Serialize()).UShortPrefixed);
    }

    [Fact]
    public void UShortPrefix_TwoByteLength()
    {
        var data = new UShortPrefixPacket { UShortPrefixed = "XY" }.Serialize();
        Assert.Equal([0x02, 0x00, 0x58, 0x59], data);
    }

    #endregion

    #region Type-Level String Defaults

    [Fact]
    public void TypeLevelDefaults_RoundTrip()
    {
        var original = new TypeLevelStringOptionsPacket { Name = "Test", Description = "Desc" };
        var result = TypeLevelStringOptionsPacket.Deserialize(original.Serialize());
        Assert.Equal("Test", result.Name);
        Assert.Equal("Desc", result.Description);
    }

    [Fact]
    public void TypeLevelDefaults_UsesShortPrefix()
    {
        var data = new TypeLevelStringOptionsPacket { Name = "AB", Description = "CD" }.Serialize();
        Assert.Equal(8, data.Length);
        Assert.Equal([0x02, 0x00, 0x41, 0x42, 0x02, 0x00, 0x43, 0x44], data);
    }

    [Fact]
    public void TypeLevelDefaults_AsciiReplacesNonAscii()
    {
        var result = TypeLevelStringOptionsPacket.Deserialize(new TypeLevelStringOptionsPacket { Name = "caf\u00E9", Description = "Desc" }.Serialize());
        Assert.Equal("caf?", result.Name);
        Assert.Equal("Desc", result.Description);
    }

    [Fact]
    public void TypeLevelDefaults_EmptyAndLongStrings()
    {
        var original = new TypeLevelStringOptionsPacket { Name = "", Description = new string('X', 100) };
        var result = TypeLevelStringOptionsPacket.Deserialize(original.Serialize());
        Assert.Empty(result.Name);
        Assert.Equal(100, result.Description.Length);
    }

    #endregion

    #region Type-Level with Field Override

    [Fact]
    public void FieldOverride_RoundTrip()
    {
        var original = new TypeLevelWithFieldOverridePacket { AsciiShort = "Hi", Utf8Int = "Hello" };
        var result = TypeLevelWithFieldOverridePacket.Deserialize(original.Serialize());
        Assert.Equal("Hi", result.AsciiShort);
        Assert.Equal("Hello", result.Utf8Int);
    }

    [Fact]
    public void FieldOverride_UsesIntPrefix()
    {
        var data = new TypeLevelWithFieldOverridePacket { AsciiShort = "A", Utf8Int = "B" }.Serialize();
        Assert.Equal(8, data.Length);
        Assert.Equal(0x01, data[0]);
        Assert.Equal(0x00, data[1]);
        Assert.Equal(0x41, data[2]);
        Assert.Equal(0x01, data[3]);
        Assert.Equal(0x00, data[4]);
        Assert.Equal(0x00, data[5]);
        Assert.Equal(0x00, data[6]);
        Assert.Equal(0x42, data[7]);
    }

    [Fact]
    public void FieldOverride_CorrectSize()
    {
        var data = new TypeLevelWithFieldOverridePacket { AsciiShort = new string('A', 50), Utf8Int = new string('B', 50) }.Serialize();
        Assert.Equal((2 + 50) + (4 + 50), data.Length);
    }

    [Fact]
    public void FieldOverride_MixedEncodings()
    {
        var original = new TypeLevelWithFieldOverridePacket { AsciiShort = "ASCII only", Utf8Int = "\u4E16\u754C" };
        var result = TypeLevelWithFieldOverridePacket.Deserialize(original.Serialize());
        Assert.Equal("ASCII only", result.AsciiShort);
        Assert.Equal("\u4E16\u754C", result.Utf8Int);
    }

    #endregion

    #region Collections

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, new[] { 10, 20, 30, 40, 50 })]
    [InlineData(new int[] { }, new[] { 1, 2, 3, 4, 5 })]
    [InlineData(new[] { 42 }, new[] { 1, 2, 3, 4, 5 })]
    public void CollectionOptionsPacket_RoundTrip(int[] dynamicArr, int[] fixedArr)
    {
        var original = new CollectionOptionsPacket { Dynamic = dynamicArr, FixedSize = fixedArr };
        var result = CollectionOptionsPacket.Deserialize(original.Serialize());
        Assert.Equal(original.Dynamic, result.Dynamic);
        Assert.Equal(original.FixedSize, result.FixedSize);
    }

    [Fact]
    public void CollectionOptionsPacket_LargeDynamic_RoundTrip()
    {
        var original = new CollectionOptionsPacket { Dynamic = Enumerable.Range(0, 100).ToArray(), FixedSize = Enumerable.Range(0, 5).ToArray() };
        var result = CollectionOptionsPacket.Deserialize(original.Serialize());
        Assert.Equal(original.Dynamic, result.Dynamic);
        Assert.Equal(original.FixedSize, result.FixedSize);
    }

    [Fact]
    public void FixedSizeArray_AlwaysSameLength()
    {
        var data = new CollectionOptionsPacket { Dynamic = [], FixedSize = [1, 2, 3, 4, 5] }.Serialize();
        Assert.Equal(4 + (5 * 4), data.Length);
    }

    [Fact]
    public void FixedSizeArray_BoundaryValues()
    {
        var original = new CollectionOptionsPacket { Dynamic = [], FixedSize = [int.MinValue, -1, 0, 1, int.MaxValue] };
        Assert.Equal(original.FixedSize, CollectionOptionsPacket.Deserialize(original.Serialize()).FixedSize);
    }

    #endregion

    #region Property-Based PacketField

    [Fact]
    public void PacketWithProperties_RoundTrip()
    {
        var original = new PacketWithProperties { Id = 42, Name = "PropertyTest" };
        var result = PacketWithProperties.Deserialize(original.Serialize());
        Assert.Equal(42, result.Id);
        Assert.Equal("PropertyTest", result.Name);
    }

    [Fact]
    public void PacketWithProperties_SameAsFields()
    {
        var props = new PacketWithProperties { Id = 100, Name = "Test" }.Serialize();
        var fields = new SimplePacket { Id = 100, Name = "Test" }.Serialize();
        output.WriteLine($"Props: {Hex(props)}");
        output.WriteLine($"Fields: {Hex(fields)}");
        Assert.Equal(fields, props);
    }

    #endregion

    #region Empty / Ignored Fields

    [Fact]
    public void EmptyPacket_Serialize_EmptyArray()
    {
        Assert.Empty(new EmptyPacket().Serialize());
    }

    [Fact]
    public void EmptyPacket_Deserialize_DefaultStruct()
    {
        Assert.Equal(new EmptyPacket(), EmptyPacket.Deserialize([]));
    }

    [Fact]
    public void PacketWithIgnoredField_OnlySerializesMarkedFields()
    {
        var original = new PacketWithIgnoredField { Included = 42, Ignored = 999 };
        var data = original.Serialize();
        Assert.Equal(4, data.Length);
        Assert.Equal(42, BitConverter.ToInt32(data, 0));
    }

    [Fact]
    public void PacketWithIgnoredField_Deserialize_IgnoresUnmarked()
    {
        var result = PacketWithIgnoredField.Deserialize([0x2A, 0x00, 0x00, 0x00]);
        Assert.Equal(42, result.Included);
        Assert.Equal(0, result.Ignored);
    }

    #endregion

    #region Type-Level Collection Options

    [Fact]
    public void TypeLevelCollection_FixedSize_RoundTrip()
    {
        var original = new TypeLevelCollectionPacket { FixedArray = [1, 2, 3] };
        var result = TypeLevelCollectionPacket.Deserialize(original.Serialize());
        Assert.Equal(3, result.FixedArray.Length);
        Assert.Equal(original.FixedArray, result.FixedArray);
    }

    [Fact]
    public void TypeLevelCollection_NoLengthPrefix()
    {
        var data = new TypeLevelCollectionPacket { FixedArray = [10, 20, 30] }.Serialize();
        Assert.Equal(3 * 4, data.Length);
    }

    #endregion

    #region Explicit Zero vs No Attribute

    [Fact]
    public void ExplicitZeroSize_SameAsNoAttribute()
    {
        var explicitData = new ExplicitZeroSizePacket { DynamicArray = [1, 2, 3] }.Serialize();
        var noAttrData = new NoAttributeCollectionPacket { DynamicArray = [1, 2, 3] }.Serialize();
        Assert.Equal(noAttrData, explicitData);
    }

    [Theory]
    [InlineData(new[] { 5, 10, 15, 20 })]
    [InlineData(new int[] { })]
    public void ExplicitZeroSize_RoundTrip(int[] values)
    {
        var original = new ExplicitZeroSizePacket { DynamicArray = values };
        var result = ExplicitZeroSizePacket.Deserialize(original.Serialize());
        Assert.Equal(values, result.DynamicArray);
    }

    [Fact]
    public void NoAttributeCollection_RoundTrip()
    {
        var original = new NoAttributeCollectionPacket { DynamicArray = [7, 14, 21] };
        var result = NoAttributeCollectionPacket.Deserialize(original.Serialize());
        Assert.Equal([7, 14, 21], result.DynamicArray);
    }

    #endregion

    #region Truncated / Corrupted Data

    [Theory]
    [InlineData(new byte[] { 0x01, 0x02 })]
    [InlineData(new byte[] { })]
    public void Deserialize_TruncatedData_Throws(byte[] data)
    {
        Assert.Throws<InvalidOperationException>(() => OrderedPacket.Deserialize(data));
    }

    [Fact]
    public void Deserialize_TruncatedString_Throws()
    {
        var full = new StringOptionsPacket { AsciiShort = "Hello", UnicodeString = "", BytePrefixString = "", DefaultString = "" }.Serialize();
        Assert.Throws<InvalidOperationException>(() => StringOptionsPacket.Deserialize(full.Take(3).ToArray()));
    }

    [Fact]
    public void Deserialize_TruncatedArray_Throws()
    {
        var full = new CollectionOptionsPacket { Dynamic = [1, 2, 3], FixedSize = [10, 20, 30, 40, 50] }.Serialize();
        Assert.Throws<InvalidOperationException>(() => CollectionOptionsPacket.Deserialize(full.Take(10).ToArray()));
    }

    [Fact]
    public void Deserialize_CorruptedLength_Throws()
    {
        var data = new SimplePacket { Id = 1, Name = "Test" }.Serialize();
        data[4] = 100;
        Assert.Throws<InvalidOperationException>(() => SimplePacket.Deserialize(data));
    }

    #endregion

    #region Helper Consistency

    [Fact]
    public void Serialize_HelperSameAsWriterOverload()
    {
        var packet = new SimplePacket { Id = 42, Name = "Test" };
        var helperData = packet.Serialize();
        var writer = new Ceiralizer.Utils.ChunkWriter(new System.Buffers.ArrayBufferWriter<byte>());
        packet.Serialize(writer);
        Assert.Equal(writer.GetWrittenData(), helperData);
    }

    [Fact]
    public void Deserialize_HelperSameAsReaderOverload()
    {
        var data = new SimplePacket { Id = 100, Name = "Consistency" }.Serialize();
        var helperResult = SimplePacket.Deserialize(data);
        var readerResult = SimplePacket.Deserialize(new Ceiralizer.Utils.ChunkReader(data));
        Assert.Equal(helperResult.Id, readerResult.Id);
        Assert.Equal(helperResult.Name, readerResult.Name);
    }

    [Fact]
    public void MultipleSerializeCalls_Identical()
    {
        var packet = new ComplexPacket { Flags = 0xAB, Symbol = '$', Inner = new InnerPacket { X = 10.0f, Y = 20.0f }, SerializableData = new FooBar(5, "Foo"), Path = [new Vector3(0, 0, 0), new Vector3(1, 1, 1)] };
        Assert.Equal(packet.Serialize(), packet.Serialize());
    }

    #endregion
}
