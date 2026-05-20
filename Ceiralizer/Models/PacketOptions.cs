namespace Ceiralizer.Models;

public class PacketOptions
{
    public TextOptions Text = new();
    public CollectionOptions Collection = new();
}

public class TextOptions
{
    public TextEncoding Encoder = TextEncoding.UTF8;
    public PrefixType PrefixLength = PrefixType.Int;
}

public class CollectionOptions
{
    public int Size = 0;
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