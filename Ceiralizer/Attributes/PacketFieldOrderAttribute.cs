namespace Ceiralizer.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class PacketFieldOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
