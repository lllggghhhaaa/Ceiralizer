using System.Text;
using Ceiralizer;
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