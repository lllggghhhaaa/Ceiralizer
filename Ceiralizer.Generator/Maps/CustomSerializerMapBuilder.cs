using System.Collections.Generic;
using System.Collections.Immutable;
using Ceiralizer.Generator.Models;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Maps;

public static class CustomSerializerMapBuilder
{
    public static IncrementalValueProvider<ImmutableArray<Dictionary<ITypeSymbol, SerializerPair>>> Create(
        IncrementalValuesProvider<INamedTypeSymbol?> classes,
        IncrementalValueProvider<INamedTypeSymbol?> serializerInterface)
    {
        return classes
            .Combine(serializerInterface)
            .Select((pair, _) =>
            {
                var (cls, ifaceSymbol) = pair;
                var dict = new Dictionary<ITypeSymbol, SerializerPair>(SymbolEqualityComparer.Default);

                if (cls is null || ifaceSymbol is null)
                    return dict;

                foreach (var iface in cls.AllInterfaces)
                {
                    if (!SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, ifaceSymbol))
                        continue;

                    var type = iface.TypeArguments[0];
                    var name = cls.ToDisplayString();

                    dict[type] = new SerializerPair(
                        $"{name}.Serialize({{0}}, writer)",
                        $"{name}.Deserialize(reader)"
                    );
                }

                return dict;
            })
            .Collect();
    }
}