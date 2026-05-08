using System.Text;

namespace Ceiralizer.Tests;

public class CeiralizerTests
{
    private const string Title = "In The End, We All Felt Like We Ate Too Much";

    private const string Description =
        "North America should be called Russia since people are always moving so fast. Gralitica Cemeteries are just garbage dumps filled with humans I'm still upset that Tie Domi didn't name his child Tyson ";
    
    [Fact]
    public void TestIntegrity()
    {
        // Default is UNICODE, change to UTF8 to reduce the size.
        PacketSerializer.StringEncoder = Encoding.UTF8;

        // Add custom class to serialize
        TypeSerializer.Serializers.Add(typeof(Vector2), value =>
        {
            List<byte> bytes = new List<byte>();

            Vector2 pos = (Vector2) value;

            byte[] x = TypeSerializer.Serializers[typeof(int)].Invoke(pos.X);
            byte[] y = TypeSerializer.Serializers[typeof(int)].Invoke(pos.Y);

            bytes.AddRange(x);
            bytes.AddRange(y);

            return bytes.ToArray();
        });

        // And also adding deserializer
        TypeSerializer.Deserializers.Add(typeof(Vector2), data =>
        {
            int x = data.ReadInt();
            int y = data.ReadInt();

            return new Vector2(x, y);
        });

        List<byte> data = PacketSerializer.Serialize(new CeiraPacket
        {
            Op = 2,
            Value = 255,
            Ceiras = new[] { 4, 66, 95 },
            Garbage = "Lixo",
            Prefix = 'c',
            Ceirinha = new CeirinhaPacket
            {
                Ceirinha = "Ceira purinha"
            },
            Name = "Ceira",
            IsWorking = true,
            Position = new Vector2(5, 25),
            Ceirax = new Ceirax
            {
                Title = Title,
                Description = Description,
                Price = 58329.32f
            }
        }).ToList();

        CeiraPacket packet = PacketSerializer.Deserialize<CeiraPacket>(data);
        
        Assert.Equal(2, packet.Op);
        Assert.Equal(255, packet.Value);
        Assert.Equal(new[] { 4, 66, 95}, packet.Ceiras);
        Assert.NotEqual("Lixo", packet.Garbage);
        Assert.Equal('c', packet.Prefix);
        Assert.Equal("Ceira purinha", packet.Ceirinha.Ceirinha);
        Assert.Equal("Ceira", packet.Name);
        Assert.True(packet.IsWorking);
        Assert.Equal(new Vector2(5, 25), packet.Position);
        Assert.Equal(Title, packet.Ceirax.Title);
        Assert.Equal(Description, packet.Ceirax.Description);
        Assert.Equal(58329.32f, packet.Ceirax.Price);
    }
}