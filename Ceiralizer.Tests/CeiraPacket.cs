namespace Ceiralizer.Tests;

public struct CeiraPacket : IPacket
{
    [PacketField] public short Op;

    public string Garbage;

    [PacketField] public int Value;

    [PacketField] public int[] Ceiras;

    [PacketField] public char Prefix;

    [PacketField] public CeirinhaPacket Ceirinha;

    [PacketField] public string Name;

    [PacketField] public bool IsWorking;

    [PacketField] public Vector2 Position;

    [PacketField] public Ceirax Ceirax;
}

public struct CeirinhaPacket : IPacket
{
    [PacketField] public string Ceirinha;
}

public struct Vector2
{
    public int X;
    public int Y;

    public Vector2(int x = 0, int y = 0)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"{X}, {Y}";
    }
}