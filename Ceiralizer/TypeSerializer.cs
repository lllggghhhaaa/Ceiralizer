using Ceiralizer.Utils;

namespace Ceiralizer;

public static class TypeSerializer
{
    public static readonly Dictionary<Type, Action<object, ChunkWriter>> Serializers = new()
    {
        { typeof(bool), (value, writer) => writer.Write((bool)value) },
        { typeof(byte), (value, writer) => writer.Write((byte)value) },
        { typeof(sbyte), (value, writer) => writer.Write((sbyte)value) },
        { typeof(char), (value, writer) => writer.Write((char)value, PacketSerializer.StringEncoder) },
        { typeof(double), (value, writer) => writer.Write((double)value) },
        { typeof(float), (value, writer) => writer.Write((float)value) },
        { typeof(int), (value, writer) => writer.Write((int)value) },
        { typeof(uint), (value, writer) => writer.Write((uint)value) },
        { typeof(long), (value, writer) => writer.Write((long)value) },
        { typeof(ulong), (value, writer) => writer.Write((ulong)value) },
        { typeof(short), (value, writer) => writer.Write((short)value) },
        { typeof(ushort), (value, writer) => writer.Write((ushort)value) },
        {
            typeof(string), (value, writer) =>
            {
                var data = (string)value;
                var header = PacketSerializer.StringEncoder.GetByteCount(data);
                writer.Write(header);
                writer.Write(data, PacketSerializer.StringEncoder);
            }
        }
    };
    
    public static readonly Dictionary<Type, Func<ChunkReader, object?>> Deserializers = new()
    {
        { typeof(bool), data => data.ReadBool() },
        { typeof(byte), data => data.ReadByte() },
        { typeof(sbyte), data => data.ReadSByte() },
        { typeof(char), data => data.ReadChar(PacketSerializer.StringEncoder) },
        { typeof(double), data => data.ReadDouble() },
        { typeof(float), data => data.ReadFloat() },
        { typeof(int), data => data.ReadInt() },
        { typeof(uint), data => data.ReadUInt() },
        { typeof(long), data => data.ReadLong() },
        { typeof(ulong), data => data.ReadULong() },
        { typeof(short), data => data.ReadShort() },
        { typeof(ushort), data => data.ReadUShort() },
        {
            typeof(string), data =>
            {
                int count = data.ReadInt();
                return data.ReadString(PacketSerializer.StringEncoder, count);
            }
        }
    };
}