using System.Collections.Generic;
using System.Collections.Immutable;
using Ceiralizer.Generator.Models;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Maps;

public static class MapMerger
{
    public static IncrementalValueProvider<ImmutableDictionary<ITypeSymbol, string>>
        MergeSerializers(
            IncrementalValueProvider<ImmutableDictionary<ITypeSymbol, string>> native,
            IncrementalValueProvider<ImmutableArray<Dictionary<ITypeSymbol, SerializerPair>>> custom)
    {
        return native.Combine(custom).Select((pair, _) =>
        {
            var (n, c) = pair;
            var result = new Dictionary<ITypeSymbol, string>(n, SymbolEqualityComparer.Default);

            foreach (var map in c)
            foreach (var kv in map)
                result[kv.Key] = kv.Value.Serialize;

            return result.ToImmutableDictionary(SymbolEqualityComparer.Default);
        });
    }

    public static IncrementalValueProvider<ImmutableDictionary<ITypeSymbol, string>>
        MergeDeserializers(
            IncrementalValueProvider<ImmutableDictionary<ITypeSymbol, string>> native,
            IncrementalValueProvider<ImmutableArray<Dictionary<ITypeSymbol, SerializerPair>>> custom)
    {
        return native.Combine(custom).Select((pair, _) =>
        {
            var (n, c) = pair;
            var result = new Dictionary<ITypeSymbol, string>(n, SymbolEqualityComparer.Default);

            foreach (var map in c)
            foreach (var kv in map)
                result[kv.Key] = kv.Value.Deserialize;

            return result.ToImmutableDictionary(SymbolEqualityComparer.Default);
        });
    }
}