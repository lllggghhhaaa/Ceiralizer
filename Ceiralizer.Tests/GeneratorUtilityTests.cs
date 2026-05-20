using Ceiralizer.Generator.Models;
using Ceiralizer.Generator.Utils;

namespace Ceiralizer.Tests;

public class NameUtilsTests
{
    [Theory]
    [InlineData("System.Collections.Generic", "System_Collections_Generic")]
    [InlineData("List[int]", "List_int")]
    [InlineData("Dictionary`2[String, Int32]", "Dictionary`2_String, Int32")]
    [InlineData("SimplePacket", "SimplePacket")]
    [InlineData("", "")]
    [InlineData(".[]", "__")]
    [InlineData("My.Namespace.MyClass<T>", "My_Namespace_MyClass<T>")]
    public void Sanitize_ProducesExpectedResult(string input, string expected)
    {
        Assert.Equal(expected, NameUtils.Sanitize(input));
    }
}

public class CascadedOptionsTests
{
    [Fact]
    public void OnlyGlobalOptions_UsesGlobalValues()
    {
        var cascaded = CascadedOptions.Create(
            new PacketOptions { Text = { Encoder = Enum(TextEncoding.ASCII), PrefixLength = Enum(PrefixType.Byte) } },
            new PacketOptions());

        Assert.Equal("Encoding.ASCII", cascaded.GetEncodingCode());
        Assert.Equal("(byte)", cascaded.GetWriterPrefixCode());
        Assert.Equal("reader.ReadByte()", cascaded.GetReaderPrefixCode());
    }

    [Fact]
    public void OnlyFieldOptions_UsesFieldValues()
    {
        var cascaded = CascadedOptions.Create(
            new PacketOptions(),
            new PacketOptions { Text = { Encoder = Enum(TextEncoding.UTF8), PrefixLength = Enum(PrefixType.Short) } });

        Assert.Equal("Encoding.UTF8", cascaded.GetEncodingCode());
        Assert.Equal("(short)", cascaded.GetWriterPrefixCode());
        Assert.Equal("reader.ReadShort()", cascaded.GetReaderPrefixCode());
    }

    [Fact]
    public void FieldOverridesGlobal()
    {
        var cascaded = CascadedOptions.Create(
            new PacketOptions { Text = { Encoder = Enum(TextEncoding.ASCII), PrefixLength = Enum(PrefixType.Byte) } },
            new PacketOptions { Text = { Encoder = Enum(TextEncoding.Unicode) } });

        Assert.Equal("Encoding.Unicode", cascaded.GetEncodingCode());
        Assert.Equal("(byte)", cascaded.GetWriterPrefixCode());
    }

    [Fact]
    public void NoOptions_UsesDefaults()
    {
        var cascaded = CascadedOptions.Create(new PacketOptions(), new PacketOptions());
        Assert.Equal("Encoding.UTF8", cascaded.GetEncodingCode());
        Assert.Empty(cascaded.GetWriterPrefixCode());
        Assert.Equal("reader.ReadInt()", cascaded.GetReaderPrefixCode());
    }

    [Fact]
    public void CollectionSize_FieldOverridesGlobal()
    {
        var cascaded = CascadedOptions.Create(
            new PacketOptions { Collection = { Size = 10 } },
            new PacketOptions { Collection = { Size = 5 } });
        Assert.Equal(5, cascaded.Options.Collection.Size);
    }

    [Fact]
    public void PartialOverride_PreservesNonOverridden()
    {
        var cascaded = CascadedOptions.Create(
            new PacketOptions { Text = { Encoder = Enum(TextEncoding.ASCII), PrefixLength = Enum(PrefixType.Byte) }, Collection = { Size = 3 } },
            new PacketOptions { Text = { PrefixLength = Enum(PrefixType.Short) } });

        Assert.Equal("Encoding.ASCII", cascaded.GetEncodingCode());
        Assert.Equal("(short)", cascaded.GetWriterPrefixCode());
        Assert.Equal(3, cascaded.Options.Collection.Size);
    }

    [Theory]
    [InlineData(TextEncoding.ASCII, "Encoding.ASCII")]
    [InlineData(TextEncoding.UTF7, "Encoding.UTF7")]
    [InlineData(TextEncoding.UTF8, "Encoding.UTF8")]
    [InlineData(TextEncoding.Unicode, "Encoding.Unicode")]
    [InlineData(TextEncoding.UTF32, "Encoding.UTF32")]
    [InlineData(TextEncoding.BigEndianUnicode, "Encoding.BigEndianUnicode")]
    public void GetEncodingCode_AllEncodings(TextEncoding encoding, string expected)
    {
        var cascaded = new CascadedOptions(new PacketOptions { Text = { Encoder = Enum(encoding) } });
        Assert.Equal(expected, cascaded.GetEncodingCode());
    }

    [Theory]
    [InlineData(PrefixType.Byte, "(byte)", "reader.ReadByte()")]
    [InlineData(PrefixType.SByte, "(sbyte)", "reader.ReadSByte()")]
    [InlineData(PrefixType.Short, "(short)", "reader.ReadShort()")]
    [InlineData(PrefixType.UShort, "(ushort)", "reader.ReadUShort()")]
    [InlineData(PrefixType.Int, "", "reader.ReadInt()")]
    public void GetPrefixCode_AllPrefixTypes(PrefixType prefix, string expectedWriter, string expectedReader)
    {
        var cascaded = new CascadedOptions(new PacketOptions { Text = { PrefixLength = Enum(prefix) } });
        Assert.Equal(expectedWriter, cascaded.GetWriterPrefixCode());
        Assert.Equal(expectedReader, cascaded.GetReaderPrefixCode());
    }

    private static EnumValue Enum<T>(T value) where T : Enum => new(typeof(T).FullName!, value.ToString(), Convert.ToInt32(value));
}
