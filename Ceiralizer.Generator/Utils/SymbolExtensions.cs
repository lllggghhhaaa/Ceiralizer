using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ceiralizer.Generator.Models;
using Microsoft.CodeAnalysis;

namespace Ceiralizer.Generator.Utils;

internal static class SymbolExtensions
{
    public static bool HasAttribute(this ISymbol symbol, string attributeName)
        => symbol.GetAttributes().Any(a => a.AttributeClass?.Name == attributeName);

    public static PacketOptions GetPacketOptions(this ISymbol symbol)
    {
        var options = new PacketOptions();

        var stringAttr = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "PacketStringOptionsAttribute");

        if (stringAttr is not null)
            ExtractAttributeArgs(stringAttr, options, SetStringOption);

        var collAttr = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "PacketCollectionOptionsAttribute");

        if (collAttr is not null)
            ExtractAttributeArgs(collAttr, options, SetCollectionOption);

        return options;
    }

    public static int GetFieldOrder(this ISymbol symbol)
    {
        var attr = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "PacketFieldOrderAttribute");

        if (attr is null || attr.ConstructorArguments.Length == 0)
            return -1;

        return attr.ConstructorArguments[0].Value is int order ? order : -1;
    }

    private static void ExtractAttributeArgs(
        AttributeData attribute,
        PacketOptions options,
        System.Action<PacketOptions, string, object?> setter)
    {
        var ctorParams = attribute.AttributeConstructor?.Parameters
                         ?? ImmutableArray<IParameterSymbol>.Empty;

        for (var i = 0; i < attribute.ConstructorArguments.Length; i++)
        {
            var paramName = i < ctorParams.Length
                ? ctorParams[i].Name
                : $"arg{i}";

            setter(options, paramName, ParseTypedConstant(attribute.ConstructorArguments[i]));
        }

        foreach (var named in attribute.NamedArguments)
            setter(options, named.Key, ParseTypedConstant(named.Value));
    }

    private static void SetStringOption(PacketOptions options, string name, object? value)
    {
        switch (name)
        {
            case "encoder" or "Encoder" when value is EnumValue ev:
                options.Text.Encoder = ev;
                break;
            case "stringPrefixLength" or "StringPrefixLength" when value is EnumValue ev:
                options.Text.PrefixLength = ev;
                break;
        }
    }

    private static void SetCollectionOption(PacketOptions options, string name, object? value)
    {
        if (name is "size" or "Size" && value is int size)
            options.Collection.Size = size;
    }

    private static object? ParseTypedConstant(TypedConstant constant)
    {
        if (constant.IsNull)
            return null;

        return constant.Kind switch
        {
            TypedConstantKind.Primitive => constant.Value,
            TypedConstantKind.Enum      => ParseEnum(constant),
            TypedConstantKind.Type      => constant.Value,
            TypedConstantKind.Array     => constant.Values
                .Select(ParseTypedConstant)
                .ToArray(),
            _                           => constant.Value
        };
    }

    private static object? ParseEnum(TypedConstant constant)
    {
        if (constant.Type is not INamedTypeSymbol enumType)
            return constant.Value;

        var member = enumType.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.IsConst && Equals(f.ConstantValue, constant.Value));

        return member is not null
            ? new EnumValue(enumType.ToDisplayString(), member.Name, constant.Value)
            : constant.Value;
    }
}

public readonly struct EnumValue
{
    public readonly string TypeName;
    public readonly string MemberName;
    private readonly object? RawValue;

    public EnumValue(string typeName, string memberName, object? rawValue)
    {
        TypeName = typeName;
        MemberName = memberName;
        RawValue = rawValue;
    }
    
    public EnumValue(object enumValue)
    {
        TypeName = enumValue.GetType().FullName ?? "UnknownEnum";
        MemberName = enumValue.ToString() ?? "Unknown";
        RawValue = enumValue;
    }
    
    public T GetValue<T>() => (T)RawValue!;
    public string ToCodeExpression() => $"{TypeName}.{MemberName}";

    public override string ToString() => ToCodeExpression();
}