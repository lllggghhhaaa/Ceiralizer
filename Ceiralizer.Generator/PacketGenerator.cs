using Ceiralizer.Generator.Generator;
using Ceiralizer.Generator.Maps;
using Ceiralizer.Generator.Models;
using Ceiralizer.Generator.Providers;
using Microsoft.CodeAnalysis;

[Generator]
public class PacketGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = ClassProvider.Create(context);
        var symbols = SymbolProvider.Create(context);

        var nativeSerializers   = WriterMapBuilder.Create(symbols.Writer);
        var nativeDeserializers = ReaderMapBuilder.Create(symbols.Reader);
        var customSerializers   = CustomSerializerMapBuilder.Create(classes, symbols.SerializerInterface);

        var serializerMap   = MapMerger.MergeSerializers(nativeSerializers, customSerializers);
        var deserializerMap = MapMerger.MergeDeserializers(nativeDeserializers, customSerializers);

        var packets = PacketFilter.Create(classes, symbols.PacketInterface);

        var sharedContext = GeneratorContext.Create(
            serializerMap,
            deserializerMap,
            symbols.PacketInterface,
            symbols.SerializableInterface
        );

        context.RegisterSourceOutput(
            packets.Combine(sharedContext),
            (ctx, data) =>
            {
                var (packet, genCtx) = data;
                if (packet is null) return;

                var source = PacketCodeGenerator.Generate(packet, genCtx);
                ctx.AddSource($"{packet.Name}.g.cs", source);
            });
    }
}