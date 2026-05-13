using System.Collections.Generic;
using Ceiralizer.Generator.Models;
using Ceiralizer.Generator.Source;
using Ceiralizer.Generator.Utils;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Generator;

public static class SerializeEmitter
{
    public static void Emit(
        BlockBuilder bb,
        List<IFieldSymbol> fields,
        GeneratorContext ctx)
    {
        foreach (var field in fields)
            EmitField(bb, field.Name, field.Type, ctx);
    }

    public static void EmitHelper(BlockBuilder bb)
    {
        bb.Variable("var", "writer", "new ChunkWriter(new ArrayBufferWriter<byte>())");
        bb.Call("Serialize", "writer");
        bb.Return("writer.GetWrittenData()");
    }

    private static void EmitField(
        BlockBuilder bb,
        string name,
        ITypeSymbol type,
        GeneratorContext ctx)
    {
        if (type.IsString())
        {
            bb.Call("writer.Write", $"Encoding.UTF8.GetByteCount({name})")
                .Call("writer.Write", name, "Encoding.UTF8");
            return;
        }

        if (type.IsChar())
        {
            bb.Call("writer.Write", name, "Encoding.UTF8");
            return;
        }

        if (type is IArrayTypeSymbol array)
        {
            EmitArray(bb, name, array, ctx);
            return;
        }

        if (type.IsPacket(ctx.PacketInterface) ||
            type.IsSerializable(ctx.SerializableInterface))
        {
            bb.Line($"{name}.Serialize(writer);");
            return;
        }

        if (ctx.Serializers.TryGetValue(type, out var serializer))
        {
            bb.Line(string.Format(serializer, name) + ";");
        }
    }

    private static void EmitArray(
        BlockBuilder bb,
        string name,
        IArrayTypeSymbol array,
        GeneratorContext ctx)
    {
        bb.Line($"writer.Write({name}.Length);");

        bb.Foreach($"var item in {name}", inner =>
            EmitField(inner, "item", array.ElementType, ctx));
    }
}