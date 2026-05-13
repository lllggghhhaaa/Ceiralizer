using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Models;

public readonly record struct SymbolBundle(
    IncrementalValueProvider<INamedTypeSymbol?> Writer,
    IncrementalValueProvider<INamedTypeSymbol?> Reader,
    IncrementalValueProvider<INamedTypeSymbol?> SerializerInterface,
    IncrementalValueProvider<INamedTypeSymbol?> PacketInterface,
    IncrementalValueProvider<INamedTypeSymbol?> SerializableInterface)
{
    public IncrementalValueProvider<INamedTypeSymbol?> Writer { get; } = Writer;
    public IncrementalValueProvider<INamedTypeSymbol?> Reader { get; } = Reader;
    public IncrementalValueProvider<INamedTypeSymbol?> SerializerInterface { get; } = SerializerInterface;
    public IncrementalValueProvider<INamedTypeSymbol?> PacketInterface { get; } = PacketInterface;
    public IncrementalValueProvider<INamedTypeSymbol?> SerializableInterface { get; } = SerializableInterface;
}