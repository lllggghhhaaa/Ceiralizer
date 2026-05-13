using System.Linq;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Utils;

public static class TypeSymbolExtensions
{
    public static bool IsPacket(this ITypeSymbol type, INamedTypeSymbol? packetInterface)
    {
        return packetInterface is not null &&
               type is INamedTypeSymbol named &&
               named.AllInterfaces.Any(i =>
                   SymbolEqualityComparer.Default.Equals(i, packetInterface));
    }

    public static bool IsSerializable(this ITypeSymbol type, INamedTypeSymbol? serializableInterface)
    {
        return serializableInterface is not null &&
               type is INamedTypeSymbol named &&
               named.AllInterfaces.Any(i =>
                   SymbolEqualityComparer.Default.Equals(i, serializableInterface));
    }
    
    public static bool IsString(this ITypeSymbol type)
        => type.SpecialType == SpecialType.System_String;

    public static bool IsChar(this ITypeSymbol type)
        => type.SpecialType == SpecialType.System_Char;
}