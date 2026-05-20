using Ceiralizer.Generator.Utils;

namespace Ceiralizer.Generator.Models;

public class PacketOptions
{
    public TextOptions Text = new();
    public CollectionOptions Collection = new();
}

public class TextOptions
{
    public EnumValue? Encoder;
    public EnumValue? PrefixLength;
}

public class CollectionOptions
{
    public int? Size;
}

public enum TextEncoding
{
    ASCII,
    UTF7,
    UTF8, 
    UTF32, 
    Unicode,
    BigEndianUnicode
}

public enum PrefixType
{
    Byte,
    SByte,
    Short,
    UShort,
    Int
}