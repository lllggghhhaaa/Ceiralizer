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
}

public struct CeirinhaPacket : IPacket
{
    [PacketField] public string Ceirinha;
}
```

### Transforming
```c#
// Declaration
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

// Serializing
IEnumerable<byte> data = PacketSerializer.Serialize(ceira);

// Deserializing
CeiraPacket packet = PacketSerializer.Deserialize<CeiraPacket>(data);

// Printing
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