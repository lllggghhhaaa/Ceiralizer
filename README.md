# Ceiralizer

Serialize structs and classes to byte array and vice-versa

## Usage

### Declaration
```c#
public struct CeiraPacket : IPacket
{
    // PacketFieldAttribute determine if the field is serializable
    [PacketField] public short Op;

    public string Garbage;

    [PacketField] public int Value;

    [PacketField] public char Prefix;

    [PacketField] public CeirinhaPacket Ceirinha;

    [PacketField] public string Name;
    
    [PacketField] public bool IsWorking;
}

public struct CeirinhaPacket : IPacket
{
    [PacketField] public string Ceirinha;
}
```

### Transforming
```c#
using System.Text;
using Ceiralizer;
using Ceiralizer.Test;

// Default is UNICODE, change to UTF8 to reduce the size.
PacketSerializer.StringEncoder = Encoding.UTF8;

// Add custom class to serialize
TypeSerializer.Serializers.Add(typeof(Vector2), value =>
{
    List<byte> bytes = new List<byte>();

    Vector2 pos = (Vector2) value;

    byte[] x = TypeSerializer.Serializers[typeof(int)].Invoke(pos.X);
    byte[] y = TypeSerializer.Serializers[typeof(int)].Invoke(pos.Y);

    bytes.AddRange(x);
    bytes.AddRange(y);

    return bytes.ToArray();
});

// And also adding deserializer
TypeSerializer.Deserializers.Add(typeof(Vector2), data =>
{
    int x = data.ReadInt();
    int y = data.ReadInt();

    return new Vector2(x, y);
});

List<byte> data = PacketSerializer.Serialize(new CeiraPacket
{
    Op = 2,
    Value = 255,
    Garbage = "Lixo",
    Prefix = 'c',
    Ceirinha = new CeirinhaPacket
    {
        Ceirinha = "Ceira purinha"
    },
    Name = "Ceira",
    IsWorking = true,
    Position = new Vector2(5, 25)
}).ToList();

CeiraPacket packet = PacketSerializer.Deserialize<CeiraPacket>(data);

Console.WriteLine(packet.Op);
Console.WriteLine(packet.Value);
Console.WriteLine(packet.Prefix);
Console.WriteLine(packet.Ceirinha.Ceirinha);
Console.WriteLine(packet.Name);
Console.WriteLine(packet.IsWorking);
Console.WriteLine($"X: {packet.Position.X}, Y: {packet.Position.Y}");
```

### Output
```text
2
255
c
Ceira purinha
Ceira
True
X: 5, Y: 25
```

## Supported types

- `bool` - (System.Boolean)
- `byte` - (System.Byte)
- `sbyte` - (System.SByte)
- `char` - (System.Char)
- `double` - (System.Double)
- `float` - (System.Single)
- `int` - (System.Int32)
- `uint` - (System.UInt32)
- `long` - (System.Int64)
- `ulong` - (System.UInt64)
- `short` - (System.Int16)
- `ushort` - (System.UInt16)
- `string` - (System.String)

## Data Structure

```text
1   | 00000010 00000000                   | 2
2   | 11111111 00000000 00000000 00000000 | 255
3   | 01100011                            | 'c'
4.1 | 00001101 00000000 00000000 00000000 | 13
4.2 | 01000011 01100101 01101001 01110010 | "Ceir"
4.2 | 01100001 00100000 01110000 01110101 | "a pu"
4.2 | 01110010 01101001 01101110 01101000 | "rinh"
4.2 | 01100001                            | "a"
5.1 | 00000101 00000000 00000000 00000000 | 5
5.2 | 01000011 01100101 01101001 01110010 | "Ceir"
5.2 | 01100001                            | "a"
6   | 00000001                            | True
7   | 00000101 00000000 00000000 00000000 | 5
7   | 00011001 00000000 00000000 00000000 | 25

01110010 01100001 00000001 000001
01 00000000 00000000 00000000 00011001 00000000 00000000 00000000
```

*The bytes seem inverted, why the computer reads the list from left to right.

<br/>

1: `short` 00000000 00000010 | 2 
```c#
[PacketField] public short Op;
```

<br/>

*Note that the Garbage has been ignored

2: `int` 11111111 00000000 00000000 00000000 | 255
```c#
[PacketField] public int Value;
```

<br/>

3: `char` 01100011 | 'c'
```c#
[PacketField] public char Prefix;
```

<br/>

4: `CeirinhaPacket`
```c#
[PacketField] public CeirinhaPacket Ceirinha;
```

*This Packet is merged to main packet

```c#
[PacketField] public string Ceirinha;
```

4.1: `int` 00001101 00000000 00000000 00000000 | 13

*As strings have no fixed length, an integer comes together to represent their size

4.2: `string` 01000011 01100101 01101001 01110010 01100001 00100000 01110000 01110101 01110010 01101001 01101110 01101000 01100001 "Ceira purinha"

<br/>

5: `serialized string`
```c#
[PacketField] public string Name;
```

5.1: `int` 00000101 00000000 00000000 00000000 | 5

5.2: `string` 01000011 01100101 01101001 01110010 01100001 | "Ceira"

<br/>

6: `bool` 00000001 | True
```c#
[PacketField] public bool IsWorking;
```

*Booleans can be 00000000 (False) or 00000001 (True), they represent a byte so as not to interfere with the reading of the packet.

7: `Vector2`
```c#
public struct Vector2
{
    public int X;
    public int Y;

    public Vector2(int x = 0, int y = 0)
    {
        X = x;
        Y = y;
    }
}

// Declaration in the packet
[PacketField] public Vector2 Position;

// Serialization
TypeSerializer.Serializers.Add(typeof(Vector2), value =>
{
    List<byte> bytes = new List<byte>();

    Vector2 pos = (Vector2) value;

    byte[] x = TypeSerializer.Serializers[typeof(int)].Invoke(pos.X);
    byte[] y = TypeSerializer.Serializers[typeof(int)].Invoke(pos.Y);

    bytes.AddRange(x);
    bytes.AddRange(y);

    return bytes.ToArray();
});

// Deserialization
TypeSerializer.Deserializers.Add(typeof(Vector2), data =>
{
    int x = data.ReadInt();
    int y = data.ReadInt();

    return new Vector2(x, y);
});
```

00000101 00000000 00000000 00000000 | 5

00011001 00000000 00000000 00000000 | 25