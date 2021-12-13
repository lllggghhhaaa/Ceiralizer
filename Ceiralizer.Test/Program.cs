using Ceiralizer;
using Ceiralizer.Test;

IEnumerable<byte> data = PacketSerializer.Serialize(new CeiraPacket
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

CeiraPacket packet = PacketSerializer.Deserialize<CeiraPacket>(data);

Console.WriteLine(packet.Op);
Console.WriteLine(packet.Value);
Console.WriteLine(packet.Prefix);
Console.WriteLine(packet.Ceirinha.Ceirinha);
Console.WriteLine(packet.Name);