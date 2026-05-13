using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Maps;

public static class ReaderMapBuilder
{
    public static IncrementalValueProvider<ImmutableDictionary<ITypeSymbol, string>> Create(
        IncrementalValueProvider<INamedTypeSymbol?> reader)
    {
        return reader.Select((symbol, _) =>
        {
            var dict = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);

            if (symbol is null)
                return dict.ToImmutableDictionary(SymbolEqualityComparer.Default);

            foreach (var method in symbol.GetMembers().OfType<IMethodSymbol>())
            {
                if (!method.Name.StartsWith("Read") || method.Parameters.Length != 0)
                    continue;

                dict[method.ReturnType] = $"reader.{method.Name}()";
            }

            return dict.ToImmutableDictionary(SymbolEqualityComparer.Default);
        });
    }
}