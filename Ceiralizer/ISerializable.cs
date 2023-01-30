namespace Ceiralizer;

public interface ISerializable
{
    public byte[] Serialize();

    public object Deserialize(Chunk data);
}