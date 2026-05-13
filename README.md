# Ceiralizer

Serialize structs and classes to a byte array and deserialize them back.

**NuGet:** https://www.nuget.org/packages/Ceiralizer

## Quick Start

### 1. Define Your Packet

```csharp
public struct CeiraPacket : IPacket
{
    [PacketField] public short Op;

    // Fields without PacketField are ignored.
    public string Garbage;

    [PacketField] public int Value;

    [PacketField] public int[] Ceiras;
    
    [PacketField] public char Prefix;
    
    [PacketField] public CeirinhaPacket Ceirinha;
    
    [PacketField] public string Name;

    [PacketField] public bool IsWorking;
    
    [PacketField] public Vector2 Position;
    
    [PacketField] public Ceirax Ceirax;

public struct CeirinhaPacket : IPacket
{
    [PacketField] public string Ceirinha;
}
```

public struct Vector2 : ISerializable
{
    public int X;
    public int Y;

    public void Serialize(ChunkWriter writer)
    {
        Serializers.SerializeInt(X, writer);
        Serializers.SerializeInt(Y, writer);
    }

    public void Deserialize(ChunkReader reader)
    {
        X = Serializers.DeserializeInt(reader);
        Y = Serializers.DeserializeInt(reader);
    }
}
```

### Transforming
```csharp
using System.Text;
using Ceiralizer;

// Default is Encoding.Unicode. Use UTF-8 to reduce the size for most text data.
CeiralizerConfig.StringEncoder = Encoding.UTF8;
```

### 3. Serialize & Deserialize

```csharp
// Serialize
byte[] data = PacketSerializer.Serialize(new CeiraPacket
{
    Op = 2,
    Value = 255,
    Ceiras = new[] { 4, 66, 95 },
    Garbage = "Lixo",
    Prefix = 'c',
    Ceirinha = new CeirinhaPacket { Ceirinha = "Ceira purinha" },
    Name = "Ceira",
    IsWorking = true,
    Position = new Vector2(5, 25),
    Ceirax = new Ceirax { Title = "Test", Description = "Test", Price = 10.5f }
});

// Deserialize
CeiraPacket packet = PacketSerializer.Deserialize<CeiraPacket>(data);

Console.WriteLine(packet.Op);           // Output: 2
Console.WriteLine(packet.Value);        // Output: 255
Console.WriteLine(packet.Name);         // Output: Ceira
Console.WriteLine(packet.IsWorking);    // Output: True
```

## Supported Types

- **Primitives:** `bool`, `byte`, `sbyte`, `char`
- **Integer:** `int`, `uint`, `short`, `ushort`, `long`, `ulong`
- **Float:** `float`, `double`
- **Text:** `string`
- **Collections:** Arrays of any supported type
- **Nested Packets:** Any type implementing `IPacket`
- **Custom Types:** Any type implementing `ISerializable`

## Custom Serialization

Implement `ISerializable` for complex types:

```csharp
public class Ceirax : ISerializable
{
    public string Title = "";
    public string Description = "";
    public float Price;

    public void Serialize(ChunkWriter writer)
    {
        Serializers.SerializeString(Title, writer);
        Serializers.SerializeString(Description, writer);
        Serializers.SerializeFloat(Price, writer);
    }

    public void Deserialize(ChunkReader reader)
    {
        Title = Serializers.DeserializeString(reader);
        Description = Serializers.DeserializeString(reader);
        Price = Serializers.DeserializeFloat(reader);
    }
}
```

> **Note:** Mark `ISerializable` fields with `[PacketField]` in your packet definition.

## How It Works

Ceiralizer serializes fields in declaration order:

1. **Size prefix:** Arrays and strings store their byte/element count first
2. **Sequential writing:** Each `[PacketField]` is written as-is to the byte stream
3. **Nested packets:** `IPacket` implementations are flattened (fields merged)
4. **Custom logic:** `ISerializable` types use their custom Serialize/Deserialize methods

Example byte structure for a short and an int:
```
[short: 2]          [int: 255]
00000010 00000000 | 11111111 00000000 00000000 00000000
```