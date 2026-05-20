using Ceiralizer.Models;

namespace Ceiralizer.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class PacketStringOptionsAttribute() : Attribute
{
    public PacketStringOptionsAttribute(
        TextEncoding encoder = TextEncoding.UTF8,
        PrefixType stringPrefixLength = PrefixType.Int) : this()
    {
        Encoder = encoder;
        StringPrefixLength = stringPrefixLength;
    }

    public TextEncoding Encoder { get; } = TextEncoding.UTF8;
    public PrefixType StringPrefixLength { get; } = PrefixType.Int;
}
