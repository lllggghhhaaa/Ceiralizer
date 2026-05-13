using System;
using System.Collections.Generic;
using System.Text;

namespace Ceiralizer.Generator.Source;

public class ClassBuilder(int indentSize = 4)
{
    private string _name = string.Empty;
    private string _modifier = "public";
    private string _kind = "class";
    private bool _isPartial;
    private readonly List<string> _baseTypes = [];
    private readonly List<string> _fields = [];
    private readonly List<string> _properties = [];
    private readonly List<ConstructorBuilder> _constructors = [];
    private readonly List<(string Header, BlockBuilder Body)> _methods = [];

    public ClassBuilder WithName(string name) { _name = name; return this; }
    public ClassBuilder WithModifier(string modifier) { _modifier = modifier; return this; }
    public ClassBuilder Partial() { _isPartial = true; return this; }
    public ClassBuilder AsStruct() { _kind = "struct"; return this; }
    public ClassBuilder Inherits(string baseType) { _baseTypes.Add(baseType); return this; }

    public ClassBuilder AddField(string modifier, string type, string name, string? value = null)
    {
        _fields.Add(value is null ? $"{modifier} {type} {name};" : $"{modifier} {type} {name} = {value};");
        return this;
    }

    public ClassBuilder AddProperty(string modifier, string type, string name,
        string? getter = "get;", string? setter = "set;")
    {
        var g = getter is null ? "" : $" {getter}";
        var s = setter is null ? "" : $" {setter}";
        _properties.Add($"{modifier} {type} {name} {{{g}{s} }}");
        return this;
    }

    public ClassBuilder AddConstructor(Action<ConstructorBuilder> configure)
    {
        var ctor = new ConstructorBuilder(_name, indentSize);
        configure(ctor);
        _constructors.Add(ctor);
        return this;
    }

    public ClassBuilder AddMethod(string modifier, string returnType, string name,
        string[] parameters, Action<BlockBuilder> body)
    {
        var header = $"{modifier} {returnType} {name}({string.Join(", ", parameters)})";
        var block = new BlockBuilder(indentSize);
        body(block);
        _methods.Add((header, block));
        return this;
    }

    public string Build(int indentLevel = 0)
    {
        var indent = new string(' ', indentLevel * indentSize);
        var inner = new string(' ', (indentLevel + 1) * indentSize);
        var sb = new StringBuilder();

        var partial = _isPartial ? "partial " : "";
        var bases = _baseTypes.Count > 0 ? $" : {string.Join(", ", _baseTypes)}" : "";

        sb.AppendLine($"{indent}{_modifier} {partial}{_kind} {_name}{bases}");
        sb.AppendLine($"{indent}{{");

        bool needsBlank = false;

        void MaybeBlank() { if (needsBlank) sb.AppendLine(); needsBlank = true; }

        foreach (var field in _fields)
        {
            MaybeBlank();
            sb.AppendLine($"{inner}{field}");
        }

        foreach (var prop in _properties)
        {
            MaybeBlank();
            sb.AppendLine($"{inner}{prop}");
        }

        foreach (var ctor in _constructors)
        {
            MaybeBlank();
            sb.Append(ctor.Build(indentLevel + 1));
        }

        foreach (var (header, body) in _methods)
        {
            MaybeBlank();
            sb.AppendLine($"{inner}{header}");
            sb.AppendLine($"{inner}{{");
            sb.Append(body.Build(indentLevel + 2));
            sb.AppendLine($"{inner}}}");
        }

        sb.AppendLine($"{indent}}}");
        return sb.ToString();
    }
}