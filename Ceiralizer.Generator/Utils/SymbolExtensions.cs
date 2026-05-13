using System.Linq;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Utils;

internal static class SymbolExtensions
{
    public static bool HasAttribute(this ISymbol symbol, string attributeName)
        => symbol.GetAttributes().Any(a => a.AttributeClass?.Name == attributeName);
}