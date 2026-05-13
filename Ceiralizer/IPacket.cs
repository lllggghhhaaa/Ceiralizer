namespace Ceiralizer;

public interface IPacket;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class PacketFieldAttribute : Attribute;

public interface ICustomSerializer<T>;
