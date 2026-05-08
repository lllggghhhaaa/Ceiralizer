using System.Buffers;
using System.Reflection;
using System.Text;
using Ceiralizer.Utils;

namespace Ceiralizer;

public static class PacketSerializer
{
    /// <summary>
    /// Encoder used to serialize and deserialize strings and chars
    /// </summary>
    public static Encoding StringEncoder = Encoding.Unicode;
    
    /// <summary>
    /// Serialize an class to byte list
    /// </summary>
    /// <param name="packet">Packet instance to serialize</param>
    /// <typeparam name="T">Packet generic type</typeparam>
    /// <returns>byte enumerable with the packet content</returns>
    public static byte[] Serialize<T>(T packet) where T : IPacket, new() 
        => RawSerialize(packet, new ChunkWriter(new ArrayBufferWriter<byte>()));

    private static byte[] RawSerialize(object packet, ChunkWriter writer)
    {
        if (packet is null) throw new ArgumentNullException(nameof(packet), "Packet is null");

        Type packetType = packet.GetType();
        FieldInfo[] fields = packetType.GetFields();

        foreach (FieldInfo field in fields)
        {
            PacketFieldAttribute? attribute = field.GetCustomAttribute<PacketFieldAttribute>();
            if (attribute is null) continue;

            object? value = field.GetValue(packet);
            if (value is null)
                throw new InvalidOperationException(
                    $"Field '{packetType.Name}.{field.Name}' is marked with PacketFieldAttribute but its value is null.");

            if (typeof(IPacket).IsAssignableFrom(field.FieldType))
                RawSerialize(value, writer);
            else
                WriteDataFromField(value, field.FieldType, writer);
        }

        return writer.GetWrittenData();
    }

    /// <summary>
    /// Deserialize an byte list to an class instance
    /// </summary>
    /// <param name="data">Packet data</param>
    /// <typeparam name="T">Packet generic type</typeparam>
    /// <returns>Packet instance of the byte list data</returns>
    public static T Deserialize<T>(IEnumerable<byte> data) where T : IPacket, new()
    {
        byte[] enumerable = data as byte[] ?? data.ToArray();
        
        ChunkReader reader = new ChunkReader(enumerable);

        object packet = new T();
        Type packetType = typeof(T);
        FieldInfo[] fields = packetType.GetFields();

        foreach (FieldInfo field in fields)
        {
            PacketFieldAttribute? attribute = field.GetCustomAttribute<PacketFieldAttribute>();
            if (attribute is null) continue;

            Type type = field.FieldType;

            object? value = typeof(IPacket).IsAssignableFrom(field.FieldType) ? RawDeserialize(reader, type) : ValueFromByteArray(reader, type);

            field.SetValue(packet, value);
        }

        return (T) packet;
    }

    private static object? RawDeserialize(ChunkReader reader, Type packetType)
    {
        object? packet = Activator.CreateInstance(packetType);
        FieldInfo[] fields = packetType.GetFields();

        foreach (FieldInfo field in fields)
        {
            PacketFieldAttribute? attribute = field.GetCustomAttribute<PacketFieldAttribute>();
            if (attribute is null) continue;

            Type type = field.FieldType;

            object? value = ValueFromByteArray(reader, type);

            field.SetValue(packet, value);
        }

        return packet;
    }

    private static void WriteDataFromField(object? value, Type type, ChunkWriter writer)
    {
        if (value is null)
            return;

        if (type.IsArray)
        {
            Type elementType = type.GetElementType()!;

            if (value is not Array values)
                return;

            writer.Write(values.Length);

            if (typeof(ISerializable).IsAssignableFrom(elementType))
            {
                foreach (object item in values)
                    ((ISerializable)item).Serialize(writer);

                return;
            }

            if (!TypeSerializer.Serializers.TryGetValue(elementType, out Action<object, ChunkWriter>? serializer))
                return;

            foreach (object item in values)
                serializer(item, writer);

            return;
        }

        if (typeof(ISerializable).IsAssignableFrom(type))
        {
            ((ISerializable)value).Serialize(writer);
            return;
        }

        if (TypeSerializer.Serializers.TryGetValue(type, out Action<object, ChunkWriter>? valueSerializer))
            valueSerializer(value, writer);
    }

    private static object? ValueFromByteArray(ChunkReader reader, Type type)
    {
        if (type.IsArray)
        {
            Type elementType = type.GetElementType()!;
            int length = reader.ReadInt();

            Array values = Array.CreateInstance(elementType, length);

            if (typeof(ISerializable).IsAssignableFrom(elementType))
            {
                for (int i = 0; i < length; i++)
                {
                    object item = Activator.CreateInstance(elementType)!;
                    ((ISerializable)item).Deserialize(reader);
                    values.SetValue(item, i);
                }

                return values;
            }

            if (!TypeSerializer.Deserializers.TryGetValue(elementType, out Func<ChunkReader, object?>? deserializer))
                return null;

            for (int i = 0; i < length; i++)
                values.SetValue(deserializer(reader), i);

            return values;
        }

        if (typeof(ISerializable).IsAssignableFrom(type))
        {
            object o = Activator.CreateInstance(type)!;
            (o as ISerializable)!.Deserialize(reader);

            return o;
        }

        if (!TypeSerializer.Deserializers.TryGetValue(type, out Func<ChunkReader, object?>? valueDeserializer))
            return null;

        return valueDeserializer(reader);
    }
}