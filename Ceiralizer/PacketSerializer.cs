using System.Reflection;
using System.Text;

namespace Ceiralizer;

public static class PacketSerializer
{
    public static Encoding StringEncoder = Encoding.Unicode;
    
    public static IEnumerable<byte> Serialize<T>(T packet) where T : IPacket, new()
        => RawSerialize(packet);

    private static IEnumerable<byte> RawSerialize(object packet)
    {
        if (packet is null) throw new ArgumentNullException(nameof(packet), "Packet is null");

        List<byte> data = new List<byte>();

        Type packetType = packet.GetType();
        FieldInfo[] fields = packetType.GetFields();

        foreach (FieldInfo field in fields)
        {
            PacketFieldAttribute? attribute = field.GetCustomAttribute<PacketFieldAttribute>();
            if (attribute is null) continue;

            object? value = field.GetValue(packet);
            if (value is null) continue;

            if (typeof(IPacket).IsAssignableFrom(field.FieldType))
                data.AddRange(RawSerialize(value));
            else
                data.AddRange(GetDataFromField(value, field.FieldType));
        }

        return data;
    }

    public static T Deserialize<T>(IEnumerable<byte> data) where T : IPacket, new()
    {
        var enumerable = data as byte[] ?? data.ToArray();
        
        List<byte> dataList = enumerable.ToList();

        object packet = new T();
        Type packetType = typeof(T);
        FieldInfo[] fields = packetType.GetFields();

        int index = 0;

        foreach (FieldInfo field in fields)
        {
            PacketFieldAttribute? attribute = field.GetCustomAttribute<PacketFieldAttribute>();
            if (attribute is null) continue;

            Type type = field.FieldType;

            object? value;

            if (typeof(IPacket).IsAssignableFrom(field.FieldType))
            {
                value = RawDeserialize(enumerable, index, out int offset, type);
                index += offset;
            }
            else
            {
                value = ValueFromByteArray(type, out int offset, index, dataList);
                index += offset;
            }

            field.SetValue(packet, value);
        }

        return (T) packet;
    }

    private static object? RawDeserialize(IEnumerable<byte> data, int index, out int offset, Type packetType)
    {
        List<byte> dataList = data.ToList();

        object? packet = Activator.CreateInstance(packetType);
        FieldInfo[] fields = packetType.GetFields();

        offset = 0;
        
        foreach (FieldInfo field in fields)
        {
            PacketFieldAttribute? attribute = field.GetCustomAttribute<PacketFieldAttribute>();
            if (attribute is null) continue;

            Type type = field.FieldType;

            object? value = ValueFromByteArray(type, out int offset2, index, dataList);
            index += offset2;
            offset += offset2;

            field.SetValue(packet, value);
        }

        return packet;
    }

    private static byte[] GetDataFromField(object? value, Type type)
    {
        if (value is null) return Array.Empty<byte>();
        
        switch (type.FullName)
        {
            case "System.Boolean":
                return (bool) value ? new byte[] {1} : new byte[] {0};
            case "System.Byte":
            case "System.SByte":
                return new[] { (byte) value };
            case "System.Char":
                return StringEncoder.GetBytes(((char) value).ToString());
            case "System.Double":
                return BitConverter.GetBytes((double) value);
            case "System.Single":
                return BitConverter.GetBytes((float) value);
            case "System.Int32":
                return BitConverter.GetBytes((int) value);
            case "System.UInt32":
                return BitConverter.GetBytes((uint) value);
            case "System.Int64":
                return BitConverter.GetBytes((long) value);
            case "System.UInt64":
                return BitConverter.GetBytes((ulong) value);
            case "System.Int16":
                return BitConverter.GetBytes((short) value);
            case "System.UInt16":
                return BitConverter.GetBytes((ushort) value);
            
            case "System.String":
                string data = (string) value;
                List<byte> header = new List<byte>(BitConverter.GetBytes(StringEncoder.GetByteCount(data)));
                header.AddRange(StringEncoder.GetBytes(data));

                return header.ToArray();
            default:
                return Array.Empty<byte>();
        }
    }

    private static object? ValueFromByteArray(Type type, out int offset, int index, List<byte> data)
    {
        object? value = null;
        
        offset = 0;

        switch (type.FullName)
        {
            case "System.Boolean":
                offset = 1;
                value = data[index] > 0;
                break;
            case "System.Byte":
                offset = 1;
                value = data[index];
                break;
            case "System.SByte":
                offset = 1;
                value = (sbyte) data[index];
                break;
            case "System.Char":
                offset = StringEncoder.GetByteCount(" ");
                value = StringEncoder.GetString(data.GetRange(index, offset).ToArray()).First();
                break;
            case "System.Double":
                offset = 8;
                value = BitConverter.ToDouble(data.GetRange(index, offset).ToArray());
                break;
            case "System.Single":
                offset = 4;
                value = BitConverter.ToSingle(data.GetRange(index, offset).ToArray());
                break;
            case "System.Int32":
                offset = 4;
                value = BitConverter.ToInt32(data.GetRange(index, offset).ToArray());
                break;
            case "System.UInt32":
                offset = 4;
                value = BitConverter.ToUInt32(data.GetRange(index, offset).ToArray());
                break;
            case "System.Int64":
                offset = 8;
                value = BitConverter.ToInt64(data.GetRange(index, offset).ToArray());
                break;
            case "System.UInt64":
                offset = 8;
                value = BitConverter.ToUInt64(data.GetRange(index, offset).ToArray());
                break;
            case "System.Int16":
                offset = 2;
                value = BitConverter.ToInt16(data.GetRange(index, offset).ToArray());
                break;
            case "System.UInt16":
                offset = 2;
                value = BitConverter.ToUInt16(data.GetRange(index, offset).ToArray());
                break;
            
            case "System.String":
                offset = 4;
                int count = BitConverter.ToInt32(data.GetRange(index, offset).ToArray());

                value = StringEncoder.GetString(data.GetRange(index + offset, count).ToArray());
                offset += count;
                break;
        }

        return value;
    }
}