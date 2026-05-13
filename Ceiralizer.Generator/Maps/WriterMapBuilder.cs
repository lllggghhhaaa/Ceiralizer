using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Maps;

public static class WriterMapBuilder
{
    public static IncrementalValueProvider<ImmutableDictionary<ITypeSymbol, string>> Create(
        IncrementalValueProvider<INamedTypeSymbol?> writer)
    {
        return writer.Select((symbol, _) =>
        {
            var dict = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);

            if (symbol is null)
                return dict.ToImmutableDictionary(SymbolEqualityComparer.Default);

            foreach (var method in symbol.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.Name != "Write" || method.Parameters.Length != 1)
                    continue;

                dict[method.Parameters[0].Type] = "writer.Write({0})";
            }

            return dict.ToImmutableDictionary(SymbolEqualityComparer.Default);
        });
    }
}