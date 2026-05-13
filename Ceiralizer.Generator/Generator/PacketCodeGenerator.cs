using System.Linq;
using Ceiralizer.Generator.Models;
using Ceiralizer.Generator.Utils;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Generator;

public static class PacketCodeGenerator
{
    public static string Generate(INamedTypeSymbol packet, GeneratorContext ctx)
    {
        var fields = packet.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.HasAttribute("PacketFieldAttribute"))
            .ToList();

        var ns = packet.ContainingNamespace.IsGlobalNamespace
            ? null
            : packet.ContainingNamespace.ToDisplayString();

        var sb = new SourceBuilder();

        sb.Namespace(ns)
            .Using("Ceiralizer") 
            .Using("Ceiralizer.Utils")
            .Using("System.Buffers") 
            .Using("System.Text")
            .AddClass(packet.Name, cb =>
            {
                cb.Partial().AsStruct()
                    .AddMethod("public", "void", "Serialize", ["ChunkWriter writer"],
                        b => SerializeEmitter.Emit(b, fields, ctx))
                    .AddMethod("public", "byte[]", "Serialize", [], SerializeEmitter.EmitHelper)
                    .AddMethod("public static", packet.Name, "Deserialize", ["ChunkReader reader"],
                        b => DeserializeEmitter.Emit(b, packet.Name, fields, ctx))
                    .AddMethod("public static", packet.Name, "Deserialize", ["byte[] data"],
                        DeserializeEmitter.EmitHelper);
            });

        return sb.ToString();
    }
}