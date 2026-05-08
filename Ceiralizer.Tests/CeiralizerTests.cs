using System.Text;
using Xunit.Abstractions;

namespace Ceiralizer.Tests;

public class CeiralizerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CeiralizerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private const string Title = "In The End, We All Felt Like We Ate Too Much";

    private const string Description =
        "North America should be called Russia since people are always moving so fast. Gralitica Cemeteries are just garbage dumps filled with humans I'm still upset that Tie Domi didn't name his child Tyson ";
    
    [Fact]
    public void TestIntegrity()
    {
        // Default is UNICODE, change to UTF8 to reduce the size.
        PacketSerializer.StringEncoder = Encoding.UTF8;

        // Add custom class to serialize
        TypeSerializer.Serializers.Add(typeof(Vector2), (value, writer) =>
        {
            Vector2 pos = (Vector2) value;

            writer.Write(pos.X);
            writer.Write(pos.Y);
        });

        // And also adding deserializer
        TypeSerializer.Deserializers.Add(typeof(Vector2), reader =>
        {
            int x = reader.ReadInt();
            int y = reader.ReadInt();

            return new Vector2(x, y);
        });

        var data = PacketSerializer.Serialize(new CeiraPacket
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
        });

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