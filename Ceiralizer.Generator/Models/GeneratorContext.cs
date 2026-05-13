using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Models;

public sealed record GeneratorContext(
    ImmutableDictionary<ITypeSymbol, string> Serializers,
    ImmutableDictionary<ITypeSymbol, string> Deserializers,
    INamedTypeSymbol? PacketInterface,
    INamedTypeSymbol? SerializableInterface)
{
    public static IncrementalValueProvider<GeneratorContext> Create(
        IncrementalValueProvider<ImmutableDictionary<ITypeSymbol, string>> serializers,
        IncrementalValueProvider<ImmutableDictionary<ITypeSymbol, string>> deserializers,
        IncrementalValueProvider<INamedTypeSymbol?> packet,
        IncrementalValueProvider<INamedTypeSymbol?> serializable)
    {
        return serializers
            .Combine(deserializers)
            .Combine(packet)
            .Combine(serializable)
            .Select((x, _) =>
            {
                var (((s, d), p), ser) = x;
                return new GeneratorContext(s, d, p, ser);
            });
    }

    public ImmutableDictionary<ITypeSymbol, string> Serializers { get; } = Serializers;
    public ImmutableDictionary<ITypeSymbol, string> Deserializers { get; } = Deserializers;
    public INamedTypeSymbol? PacketInterface { get; } = PacketInterface;
    public INamedTypeSymbol? SerializableInterface { get; } = SerializableInterface;
}