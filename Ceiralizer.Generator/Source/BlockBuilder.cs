using System;
using System.Collections.Generic;
using System.Text;

namespace Ceiralizer.Generator.Source;

public class BlockBuilder(int indentSize = 4)
{
    private sealed record RawLine(string Text)
    {
        public string Text { get; } = Text;
    }

    private sealed record ControlFlow(string Keyword, BlockBuilder Body)
    {
        public string Keyword { get; } = Keyword;
        public BlockBuilder Body { get; } = Body;
    }

    private sealed record Scope(BlockBuilder Body)
    {
        public BlockBuilder Body { get; } = Body;
    }

    private readonly List<object> _items = [];

    public BlockBuilder Variable(string type, string name, string? value = null)
        => Emit(value is null ? $"{type} {name};" : $"{type} {name} = {value};");

    public BlockBuilder Assign(string target, string value)
        => Emit($"{target} = {value};");

    public BlockBuilder Call(string method, params string[] args)
        => Emit($"{method}({string.Join(", ", args)});");

    public BlockBuilder Return(string? expression = null)
        => Emit(expression is null ? "return;" : $"return {expression};");

    public BlockBuilder Comment(string comment)
    {
        if (!comment.Contains("\n"))
            return Emit($"// {comment}");

        Emit("/*");
        foreach (var line in comment.Split('\n'))
            Emit($" * {line}");
        return Emit(" */");
    }
    
    public BlockBuilder Line(string line)
        => Emit(line);

    public BlockBuilder BlankLine()
        => Emit(string.Empty);

    public BlockBuilder For(string header, Action<BlockBuilder> body)
        => ControlBlock($"for ({header})", body);

    public BlockBuilder Foreach(string header, Action<BlockBuilder> body)
        => ControlBlock($"foreach ({header})", body);

    public BlockBuilder If(string condition, Action<BlockBuilder> body)
        => ControlBlock($"if ({condition})", body);

    /// <summary>Creates an anonymous scope {{ }} without a control-flow keyword.</summary>
    public BlockBuilder Block(Action<BlockBuilder> body)
    {
        var inner = new BlockBuilder(indentSize);
        body(inner);
        _items.Add(new Scope(inner));
        return this;
    }

    public string Build(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        var indent = new string(' ', indentLevel * indentSize);

        foreach (var item in _items)
        {
            switch (item)
            {
                case RawLine { Text: var text } when string.IsNullOrWhiteSpace(text):
                    sb.AppendLine();
                    break;

                case RawLine { Text: var text }:
                    foreach (var line in text.Split('\n'))
                        sb.AppendLine(string.IsNullOrWhiteSpace(line) ? "" : $"{indent}{line.Trim()}");
                    break;

                case ControlFlow { Keyword: var kw, Body: var body }:
                    sb.AppendLine($"{indent}{kw}");
                    sb.AppendLine($"{indent}{{");
                    sb.Append(body.Build(indentLevel + 1));
                    sb.AppendLine($"{indent}}}");
                    break;

                case Scope { Body: var body }:
                    sb.AppendLine($"{indent}{{");
                    sb.Append(body.Build(indentLevel + 1));
                    sb.AppendLine($"{indent}}}");
                    break;
            }
        }

        return sb.ToString();
    }

    private BlockBuilder Emit(string line)
    {
        _items.Add(new RawLine(line));
        return this;
    }

    private BlockBuilder ControlBlock(string keyword, Action<BlockBuilder> body)
    {
        var inner = new BlockBuilder(indentSize);
        body(inner);
        _items.Add(new ControlFlow(keyword, inner));
        return this;
    }
}