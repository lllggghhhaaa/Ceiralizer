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
        List<PacketCodeGenerator.MemberInfo> members,
        PacketOptions globalOptions,
        GeneratorContext ctx)
    {
        foreach (var member in members)
        {
            var fieldOptions = member.Options;
            var cascadedOptions = CascadedOptions.Create(globalOptions, fieldOptions);
            EmitField(bb, member.Name, member.Type, cascadedOptions, ctx);
        }
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
        CascadedOptions options,
        GeneratorContext ctx)
    {
        if (type.IsString())
        {
            bb.Call("writer.Write", $"{options.GetWriterPrefixCode()}({options.GetEncodingCode()}).GetByteCount({name})")
                .Call("writer.Write", name, options.GetEncodingCode());
            return;
        }

        if (type.IsChar())
        {
            bb.Call("writer.Write", name, options.GetEncodingCode());
            return;
        }

        if (type is IArrayTypeSymbol array)
        {
            EmitArray(bb, name, array, options, ctx);
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
        CascadedOptions options,
        GeneratorContext ctx)
    {
        var size = options.Options.Collection.Size;

        if (size is > 0)
        {
            var idx = $"{NameUtils.Sanitize(name)}_i";
            bb.For($"int {idx} = 0; {idx} < {size.Value}; {idx}++", inner =>
                EmitField(inner, $"{name}[{idx}]", array.ElementType, options, ctx));
        }
        else
        {
            bb.Line($"writer.Write({name}.Length);");
            bb.Foreach($"var item in {name}", inner =>
                EmitField(inner, "item", array.ElementType, options, ctx));
        }
    }
}