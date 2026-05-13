using System.Collections.Generic;
using Ceiralizer.Generator.Models;
using Ceiralizer.Generator.Source;
using Ceiralizer.Generator.Utils;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Generator;

public static class DeserializeEmitter
{
    public static void Emit(
        BlockBuilder bb,
        string typeName,
        List<IFieldSymbol> fields,
        GeneratorContext ctx)
    {
        bb.Variable("var", "packet", $"new {typeName}()");

        foreach (var field in fields)
        {
            EmitField(
                bb,
                $"packet.{field.Name}",
                field.Type,
                field.Type.ToDisplayString(),
                ctx);
        }

        bb.Return("packet");
    }

    public static void EmitHelper(BlockBuilder bb)
    {
        bb.Variable("var", "reader", "new ChunkReader(data)");
        bb.Return("Deserialize(reader)");
    }

    private static void EmitField(
        BlockBuilder bb,
        string target,
        ITypeSymbol type,
        string display,
        GeneratorContext ctx)
    {
        if (type.IsString())
        {
            bb.Assign(target, "reader.ReadString(Encoding.UTF8, reader.ReadInt())");
            return;
        }

        if (type.IsChar())
        {
            bb.Assign(target, "reader.ReadChar(Encoding.UTF8)");
            return;
        }

        if (type is IArrayTypeSymbol array)
        {
            EmitArray(bb, target, array, ctx);
            return;
        }

        if (type.IsPacket(ctx.PacketInterface))
        {
            bb.Assign(target, $"{display}.Deserialize(reader)");
            return;
        }

        if (type.IsSerializable(ctx.SerializableInterface))
        {
            var local = NameUtils.Sanitize(target);

            bb.Variable("var", local, $"new {display}()")
              .Line($"{local}.Deserialize(reader);")
              .Assign(target, local);

            return;
        }

        if (ctx.Deserializers.TryGetValue(type, out var deser))
        {
            bb.Assign(target, deser);
        }
    }

    private static void EmitArray(
        BlockBuilder bb,
        string target,
        IArrayTypeSymbol array,
        GeneratorContext ctx)
    {
        var len = $"{NameUtils.Sanitize(target)}_len";
        var i   = $"{NameUtils.Sanitize(target)}_i";
        var elementType = array.ElementType.ToDisplayString();

        bb.Variable("var", len, "reader.ReadInt()")
          .Assign(target, $"new {elementType}[{len}]")
          .For($"int {i} = 0; {i} < {len}; {i}++", inner =>
              EmitField(inner, $"{target}[{i}]", array.ElementType, elementType, ctx));
    }
}