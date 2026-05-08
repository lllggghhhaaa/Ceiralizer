using Ceiralizer.Utils;

namespace Ceiralizer;

public interface ISerializable
{
    public void Serialize(ChunkWriter writer);

    public void Deserialize(ChunkReader reader);
}