# Ceiralizer

A C# source-generated serialization library for converting structs and classes into compact binary data and back. Designed for network packet serialization.

**NuGet:**
- https://www.nuget.org/packages/Ceiralizer
- https://www.nuget.org/packages/Ceiralizer.Generator (source generator — required alongside the main package)

## How It Works

Ceiralizer uses a **Roslyn incremental source generator** to automatically emit `Serialize()` and `Deserialize()` methods at compile time for any type implementing `IPacket`. Just mark fields with `[PacketField]` and make the type `partial`.

## Quick Start

### 1. Define Your Packet

```csharp
using Ceiralizer;
using Ceiralizer.Attributes;

public partial struct MyPacket : IPacket
{
    [PacketField] public int Id;
    [PacketField] public string Name;
    [PacketField] public float[] Values;
}
```

### 2. Serialize & Deserialize

```csharp
var packet = new MyPacket
{
    Id = 42,
    Name = "Hello",
    Values = [1.5f, 2.5f, 3.0f]
};

// Serialize to byte[]
byte[] data = packet.Serialize();

// Deserialize from byte[]
MyPacket result = MyPacket.Deserialize(data);

Console.WriteLine(result.Name); // Output: Hello
```

## Supported Field Types

| Category | Types |
|---|---|
| **Primitives** | `bool`, `byte`, `sbyte`, `char` |
| **Integers** | `int`, `uint`, `short`, `ushort`, `long`, `ulong` |
| **Floats** | `float`, `double` |
| **Text** | `string` (with configurable encoding/prefix) |
| **Collections** | Arrays of any supported type (dynamic or fixed-size) |
| **Nested Packets** | Any type implementing `IPacket` |
| **Custom Serializable** | Any type implementing `ISerializable` |
| **Custom Serializer** | Any type with an external `ICustomSerializer<T>` class |

## Configuration Attributes

### `[PacketFieldOrder(int order)]`

Control serialization order. Fields are sorted ascending (unmarked fields have implicit order `-1`, serialized first in declaration order).

```csharp
public partial struct OrderedPacket : IPacket
{
    [PacketField] [PacketFieldOrder(3)] public int Third;
    [PacketField] [PacketFieldOrder(1)] public int First;
    [PacketField] [PacketFieldOrder(2)] public int Second;
}
```

### `[PacketStringOptions(encoder, stringPrefixLength)]`

Control string encoding and length prefix per-field or per-type.

```csharp
using Ceiralizer.Models;

public partial struct StringOptionsPacket : IPacket
{
    [PacketField]
    [PacketStringOptions(encoder: TextEncoding.ASCII, stringPrefixLength: PrefixType.Short)]
    public string AsciiShort;

    [PacketField]
    [PacketStringOptions(encoder: TextEncoding.Unicode)]
    public string UnicodeString;
}
```

Apply to the type itself for a default:

```csharp
[PacketStringOptions(encoder: TextEncoding.ASCII, stringPrefixLength: PrefixType.Short)]
public partial struct TypeLevelPacket : IPacket
{
    [PacketField] public string Name;
}
```

**Encoding options:** `ASCII`, `UTF7`, `UTF8` (default), `UTF32`, `Unicode`, `BigEndianUnicode`

**Prefix options:** `Byte`, `SByte`, `Short`, `UShort`, `Int` (default)

### `[PacketCollectionOptions(size)]`

Control array serialization. `size = 0` (default) = dynamic (length-prefixed). `size > 0` = fixed-size (no prefix).

```csharp
public partial struct CollectionOptionsPacket : IPacket
{
    [PacketField] [PacketCollectionOptions(size: 0)] public int[] Dynamic;
    [PacketField] [PacketCollectionOptions(size: 5)] public int[] FixedSize; // exactly 5 elements
}
```

Apply to the type itself:

```csharp
[PacketCollectionOptions(size: 3)]
public partial struct FixedArrayPacket : IPacket
{
    [PacketField] public int[] Values; // always 3 elements
}
```

## Custom Serialization

### `ISerializable` — Custom logic on the type itself

Implement `ISerializable` for types that need manual serialization:

```csharp
public class FooBar : ISerializable
{
    public int A { get; set; }
    public string B { get; set; } = string.Empty;

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
}

public partial struct PacketWithSerializable : IPacket
{
    [PacketField] public FooBar Data;
}
```

### `ICustomSerializer<T>` — External serializer for types you don't control

```csharp
public readonly record struct Vector3(float X, float Y, float Z);

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
        return new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
    }
}

public partial struct PacketWithCustomSerializer : IPacket
{
    [PacketField] public Vector3 Position;
}
```

> **Note:** Fields using `ISerializable` or `ICustomSerializer<T>` must still be marked with `[PacketField]`.

## How Serialization Works

1. **Source generation:** At compile time, the generator produces `Serialize(ChunkWriter)`, `Serialize()` (returns `byte[]`), `Deserialize(ChunkReader)`, and `Deserialize(byte[])` for every `partial` type implementing `IPacket`.
2. **Field ordering:** Fields are serialized in `[PacketFieldOrder]` ascending order; unmarked fields come first in declaration order.
3. **Strings:** Stored as byte-length prefix (configurable) + encoded bytes.
4. **Arrays:** Stored as element-count prefix (`int`) for dynamic arrays, or no prefix for fixed-size arrays.
5. **Nested packets:** Fields are serialized sequentially (flattened).
6. **Byte order:** All multi-byte values are little-endian.

## Binary Format Example

For a `SimplePacket` with fields `int Id` and `string Name` ("AB"):

```
[int: 256]     [int: 2 (string byte length)]   [bytes: "AB"]
00 01 00 00  | 02 00 00 00                   | 41 42
```
