namespace Ceiralizer.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class PacketCollectionOptionsAttribute(int size = 0) : Attribute
{
    public int Size { get; } = size;
}
