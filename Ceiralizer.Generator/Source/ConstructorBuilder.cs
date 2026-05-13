using System;
using System.Collections.Generic;
using System.Text;

namespace Ceiralizer.Generator.Source;

public class ConstructorBuilder(string className, int indentSize = 4)
{
    private string _modifier = "public";
    private readonly List<string> _parameters = [];
    private readonly List<string> _baseArgs = [];
    private Action<BlockBuilder>? _body;

    public ConstructorBuilder WithModifier(string modifier) { _modifier = modifier; return this; }
    public ConstructorBuilder AddParameter(string type, string name) { _parameters.Add($"{type} {name}"); return this; }
    public ConstructorBuilder WithBase(params string[] args) { _baseArgs.AddRange(args); return this; }
    public ConstructorBuilder WithBody(Action<BlockBuilder> body) { _body = body; return this; }

    public string Build(int indentLevel = 0)
    {
        var indent = new string(' ', indentLevel * indentSize);
        var sb = new StringBuilder();

        var @params = string.Join(", ", _parameters);
        var baseCall = _baseArgs.Count > 0 ? $" : base({string.Join(", ", _baseArgs)})" : "";

        sb.AppendLine($"{indent}{_modifier} {className}({@params}){baseCall}");
        sb.AppendLine($"{indent}{{");

        if (_body is not null)
        {
            var block = new BlockBuilder(indentSize);
            _body(block);
            sb.Append(block.Build(indentLevel + 1));
        }

        sb.AppendLine($"{indent}}}");
        return sb.ToString();
    }
}