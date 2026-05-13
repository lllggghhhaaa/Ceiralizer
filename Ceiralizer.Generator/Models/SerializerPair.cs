namespace Ceiralizer.Generator.Models;

public readonly record struct SerializerPair(string Serialize, string Deserialize)
{
    public string Serialize { get; } = Serialize;
    public string Deserialize { get; } = Deserialize;
}