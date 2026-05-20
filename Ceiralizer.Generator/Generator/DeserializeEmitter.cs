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
        List<PacketCodeGenerator.MemberInfo> members,
        PacketOptions globalOptions,
        GeneratorContext ctx)
    {
        bb.Variable("var", "packet", $"new {typeName}()");

        foreach (var member in members)
        {
            var cascadedOptions = CascadedOptions.Create(globalOptions, member.Options);

            EmitField(
                bb,
                $"packet.{member.Name}",
                member.Type,
                member.Type.ToDisplayString(),
                cascadedOptions,
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
        CascadedOptions options,
        GeneratorContext ctx)
    {
        if (type.IsString())
        {
            bb.Assign(target, $"reader.ReadString({options.GetEncodingCode()}, {options.GetReaderPrefixCode()})");
            return;
        }

        if (type.IsChar())
        {
            bb.Assign(target, $"reader.ReadChar({options.GetEncodingCode()})");
            return;
        }

        if (type is IArrayTypeSymbol array)
        {
            EmitArray(bb, target, array, options, ctx);
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
        CascadedOptions options,
        GeneratorContext ctx)
    {
        var size = options.Options.Collection.Size;

        if (size is > 0)
        {
            var i = $"{NameUtils.Sanitize(target)}_i";
            var elementType = array.ElementType.ToDisplayString();

            bb.Assign(target, $"new {elementType}[{size.Value}]")
              .For($"int {i} = 0; {i} < {size.Value}; {i}++", inner =>
                  EmitField(inner, $"{target}[{i}]", array.ElementType, elementType, options, ctx));
        }
        else
        {
            var length = $"{NameUtils.Sanitize(target)}_len";
            var i = $"{NameUtils.Sanitize(target)}_i";
            var elementType = array.ElementType.ToDisplayString();

            bb.Variable("var", length, "reader.ReadInt()")
              .Assign(target, $"new {elementType}[{length}]")
              .For($"int {i} = 0; {i} < {length}; {i}++", inner =>
                  EmitField(inner, $"{target}[{i}]", array.ElementType, elementType, options, ctx));
        }
    }
}