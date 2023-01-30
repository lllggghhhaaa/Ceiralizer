namespace Ceiralizer.Tests;

public class Ceirax : ISerializable
{
    public string Title = "";
    public string Description = "";
    public float Price;

    public byte[] Serialize()
    {
        List<byte> data = new List<byte>();

        data.AddRange(TypeSerializer.Serializers[typeof(string)].Invoke(Title));
        data.AddRange(TypeSerializer.Serializers[typeof(string)].Invoke(Description));
        data.AddRange(TypeSerializer.Serializers[typeof(float)].Invoke(Price));
        
        return data.ToArray();
    }

    public void Deserialize(Chunk data)
    {
        Title = (TypeSerializer.Deserializers[typeof(string)].Invoke(data) as string)!;
        Description = (TypeSerializer.Deserializers[typeof(string)].Invoke(data) as string)!;
        Price = (float) TypeSerializer.Deserializers[typeof(float)].Invoke(data)!;
    }
}