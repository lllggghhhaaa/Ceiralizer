namespace Ceiralizer.Test;

public struct CeiraPacket : IPacket
{
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