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
// Declaration.
CeiraPacket ceira = new CeiraPacket
{
    Op = 2,
    Value = 255,
    Garbage = "Lixo",
    Prefix = 'c',
    Ceirinha = new CeirinhaPacket
    {
        Ceirinha = "Ceira purinha"
    },
    Name = "Ceira"
});

// Changing Encoder.
// Default is UNICODE, change to UTF8 to reduce the size.
PacketSerializer.StringEncoder = Encoding.UTF8;

// Serializing.
IEnumerable<byte> data = PacketSerializer.Serialize(ceira);

// Deserializing.
CeiraPacket packet = PacketSerializer.Deserialize<CeiraPacket>(data);

// Printing.
Console.WriteLine(packet.Op);
Console.WriteLine(packet.Value);
Console.WriteLine(packet.Prefix);
Console.WriteLine(packet.Ceirinha.Ceirinha);
Console.WriteLine(packet.Name);
```

### Output
```text
2
255
c
Ceira purinha
Ceira
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
```

*The bytes seem inverted, why the computer reads the list from left to right.

<br/>

1: `short` 00000000 00000010 | 2 
```c#
[PacketField] public short Op;
```

<br/>

*Note that the Gatbage has been ignored

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