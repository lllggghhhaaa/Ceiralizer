using System.Collections.Generic;
using System.Linq;
using Ceiralizer.Generator.Models;
using Ceiralizer.Generator.Utils;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Generator;

public static class PacketCodeGenerator
{
    public static string Generate(INamedTypeSymbol packet, GeneratorContext ctx)
    {
        var members = new List<MemberInfo>();

        foreach (var member in packet.GetMembers())
        {
            if (!member.HasAttribute("PacketFieldAttribute"))
                continue;

            if (member is IFieldSymbol field)
            {
                members.Add(new MemberInfo(field.Name, field.Type, field.GetFieldOrder(), field.GetPacketOptions(), isProperty: false));
            }
            else if (member is IPropertySymbol prop)
            {
                members.Add(new MemberInfo(prop.Name, prop.Type, prop.GetFieldOrder(), prop.GetPacketOptions(), isProperty: true));
            }
        }

        var ordered = members.OrderBy(m => m.Order).ToList();

        var ns = packet.ContainingNamespace.IsGlobalNamespace
            ? null
            : packet.ContainingNamespace.ToDisplayString();

        var globalOptions = packet.GetPacketOptions();

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
                        b => SerializeEmitter.Emit(b, ordered, globalOptions, ctx))
                    .AddMethod("public", "byte[]", "Serialize", [], SerializeEmitter.EmitHelper)
                    .AddMethod("public static", packet.Name, "Deserialize", ["ChunkReader reader"],
                        b => DeserializeEmitter.Emit(b, packet.Name, ordered, globalOptions, ctx))
                    .AddMethod("public static", packet.Name, "Deserialize", ["byte[] data"],
                        DeserializeEmitter.EmitHelper);
            });

        return sb.ToString();
    }

    public sealed class MemberInfo
    {
        public string Name { get; }
        public ITypeSymbol Type { get; }
        public int Order { get; }
        public PacketOptions Options { get; }
        public bool IsProperty { get; }

        public MemberInfo(string name, ITypeSymbol type, int order, PacketOptions options, bool isProperty)
        {
            Name = name;
            Type = type;
            Order = order;
            Options = options;
            IsProperty = isProperty;
        }
    }
}