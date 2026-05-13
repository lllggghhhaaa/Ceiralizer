using System.Linq;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Providers;

public static class PacketFilter
{
    public static IncrementalValuesProvider<INamedTypeSymbol?> Create(
        IncrementalValuesProvider<INamedTypeSymbol?> classes,
        IncrementalValueProvider<INamedTypeSymbol?> packetInterface)
    {
        return classes
            .Combine(packetInterface)
            .Where(pair =>
            {
                var (cls, iface) = pair;
                return cls is not null && iface is not null &&
                       cls.AllInterfaces.Any(i =>
                           SymbolEqualityComparer.Default.Equals(i, iface));
            })
            .Select((pair, _) => pair.Left);
    }
}