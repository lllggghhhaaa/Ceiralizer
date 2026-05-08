using Ceiralizer.Utils;

namespace Ceiralizer.Tests;

public class Ceirax : ISerializable
{
    public string Title = "";
    public string Description = "";
    public float Price;

    public void Serialize(ChunkWriter writer)
    {
        TypeSerializer.Serializers[typeof(string)].Invoke(Title, writer);
        TypeSerializer.Serializers[typeof(string)].Invoke(Description, writer);
        TypeSerializer.Serializers[typeof(float)].Invoke(Price, writer);
    }

    public void Deserialize(ChunkReader reader)
    {
        Title = (TypeSerializer.Deserializers[typeof(string)].Invoke(reader) as string)!;
        Description = (TypeSerializer.Deserializers[typeof(string)].Invoke(reader) as string)!;
        Price = (float) TypeSerializer.Deserializers[typeof(float)].Invoke(reader)!;
    }
}