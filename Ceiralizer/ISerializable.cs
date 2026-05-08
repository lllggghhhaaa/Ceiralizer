namespace Ceiralizer;

public interface ISerializable
{
    public byte[] Serialize();

    public void Deserialize(Chunk data);
}