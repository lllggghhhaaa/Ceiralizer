using Ceiralizer.Generator.Models;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Providers;

public static class SymbolProvider
{
    public static SymbolBundle Create(IncrementalGeneratorInitializationContext context)
    {
        var c = context.CompilationProvider;

        return new SymbolBundle(
            c.Select((x, _) => x.GetTypeByMetadataName("Ceiralizer.Utils.ChunkWriter")),
            c.Select((x, _) => x.GetTypeByMetadataName("Ceiralizer.Utils.ChunkReader")),
            c.Select((x, _) => x.GetTypeByMetadataName("Ceiralizer.ICustomSerializer`1")),
            c.Select((x, _) => x.GetTypeByMetadataName("Ceiralizer.IPacket")),
            c.Select((x, _) => x.GetTypeByMetadataName("Ceiralizer.ISerializable"))
        );
    }
}