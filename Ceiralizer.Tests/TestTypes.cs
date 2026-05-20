using System.Text;
using Ceiralizer;
using Ceiralizer.Attributes;
using Ceiralizer.Models;
using Ceiralizer.Utils;

public partial struct SimplePacket : IPacket
{
    [PacketField] public int Id;
    [PacketField] public string Name;
}

public partial struct PacketWithArray : IPacket
{
    [PacketField] public int[] Values;
    [PacketField] public string[] Names;
}

public partial struct InnerPacket : IPacket
{
    [PacketField] public float X;
    [PacketField] public float Y;
}

public partial struct OuterPacket : IPacket
{
    [PacketField] public int Count;
    [PacketField] public InnerPacket Position;
}

public class FooBar : ISerializable
{
    public int A { get; set; }
    public string B { get; set; } = string.Empty;

    public FooBar() { }

    public FooBar(int a, string b)
    {
        A = a;
        B = b;
    }

    public void Serialize(ChunkWriter writer)
    {
        writer.Write(A);
        writer.Write(B.Length);
        writer.Write(B, Encoding.UTF8);
    }

    public void Deserialize(ChunkReader reader)
    {
        A = reader.ReadInt();
        int len = reader.ReadInt();
        B = reader.ReadString(Encoding.UTF8, len);
    }

    public override bool Equals(object? obj) =>
        obj is FooBar other && A == other.A && B == other.B;

    public override int GetHashCode() => HashCode.Combine(A, B);
}

public partial struct PacketWithSerializable : IPacket
{
    [PacketField] public FooBar Data;
}

public readonly record struct Vector3(float X, float Y, float Z)
{
    public static readonly Vector3 Zero = new(0f, 0f, 0f);
}

public class Vector3Serializer : ICustomSerializer<Vector3>
{
    public static void Serialize(Vector3 vec, ChunkWriter writer)
    {
        writer.Write(vec.X);
        writer.Write(vec.Y);
        writer.Write(vec.Z);
    }

    public static Vector3 Deserialize(ChunkReader reader)
    {
        return new Vector3(
            reader.ReadFloat(),
            reader.ReadFloat(),
            reader.ReadFloat());
    }
}

public partial struct PacketWithCustomSerializer : IPacket
{
    [PacketField] public Vector3 Position;
}

public partial struct ComplexPacket : IPacket
{
    [PacketField] public byte Flags;
    [PacketField] public char Symbol;
    [PacketField] public InnerPacket Inner;
    [PacketField] public FooBar SerializableData;
    [PacketField] public Vector3[] Path;
}

public partial struct CharOnlyPacket : IPacket
{
    [PacketField] public char Ch;
}

// --- Attribute test types ---

public partial struct OrderedPacket : IPacket
{
    [PacketField] [PacketFieldOrder(3)] public int Third;
    [PacketField] [PacketFieldOrder(1)] public int First;
    [PacketField] [PacketFieldOrder(2)] public int Second;
}

public partial struct StringOptionsPacket : IPacket
{
    [PacketField] [PacketStringOptions(encoder: TextEncoding.ASCII, stringPrefixLength: PrefixType.Short)] public string AsciiShort;
    [PacketField] [PacketStringOptions(encoder: TextEncoding.Unicode)] public string UnicodeString;
    [PacketField] [PacketStringOptions(stringPrefixLength: PrefixType.Byte)] public string BytePrefixString;
    [PacketField] public string DefaultString;
}

[PacketStringOptions(encoder: TextEncoding.ASCII, stringPrefixLength: PrefixType.Short)]
public partial struct TypeLevelStringOptionsPacket : IPacket
{
    [PacketField] public string Name;
    [PacketField] public string Description;
}

[PacketStringOptions(encoder: TextEncoding.ASCII, stringPrefixLength: PrefixType.Short)]
public partial struct TypeLevelWithFieldOverridePacket : IPacket
{
    [PacketField] public string AsciiShort;
    [PacketField]
    [PacketStringOptions(encoder: TextEncoding.UTF8, stringPrefixLength: PrefixType.Int)]
    public string Utf8Int;
}

public partial struct CollectionOptionsPacket : IPacket
{
    [PacketField] [PacketCollectionOptions(size: 0)] public int[] Dynamic;
    [PacketField] [PacketCollectionOptions(size: 5)] public int[] FixedSize;
}

public partial struct Utf7StringPacket : IPacket
{
    [PacketField] [PacketStringOptions(encoder: TextEncoding.UTF7)] public string Utf7String;
}

public partial struct Utf32StringPacket : IPacket
{
    [PacketField] [PacketStringOptions(encoder: TextEncoding.UTF32)] public string Utf32String;
}

public partial struct BigEndianStringPacket : IPacket
{
    [PacketField] [PacketStringOptions(encoder: TextEncoding.BigEndianUnicode)] public string BigEndianString;
}

public partial struct SBytePrefixPacket : IPacket
{
    [PacketField] [PacketStringOptions(stringPrefixLength: PrefixType.SByte)] public string SBytePrefixed;
}

public partial struct ShortPrefixPacket : IPacket
{
    [PacketField] [PacketStringOptions(stringPrefixLength: PrefixType.Short)] public string ShortPrefixed;
}

public partial struct UShortPrefixPacket : IPacket
{
    [PacketField] [PacketStringOptions(stringPrefixLength: PrefixType.UShort)] public string UShortPrefixed;
}

public partial struct PacketWithProperties : IPacket
{
    [PacketField] public int Id { get; set; }
    [PacketField] public string Name { get; set; }
}

public partial struct EmptyPacket : IPacket
{
}

public partial struct PacketWithIgnoredField : IPacket
{
    [PacketField] public int Included;
    public int Ignored;
}

public partial struct DuplicateOrderPacket : IPacket
{
    [PacketField] [PacketFieldOrder(1)] public int First;
    [PacketField] [PacketFieldOrder(1)] public int Second;
    [PacketField] [PacketFieldOrder(2)] public int Third;
}

public partial struct NegativeOrderPacket : IPacket
{
    [PacketField] [PacketFieldOrder(-5)] public int First;
    [PacketField] [PacketFieldOrder(-10)] public int Second;
    [PacketField] [PacketFieldOrder(0)] public int Third;
}

[PacketCollectionOptions(size: 3)]
public partial struct TypeLevelCollectionPacket : IPacket
{
    [PacketField] public int[] FixedArray;
}

public partial struct ExplicitZeroSizePacket : IPacket
{
    [PacketField] [PacketCollectionOptions(size: 0)] public int[] DynamicArray;
}

public partial struct NoAttributeCollectionPacket : IPacket
{
    [PacketField] public int[] DynamicArray;
}