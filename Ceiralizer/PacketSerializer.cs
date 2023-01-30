// Copyright 2022 lllggghhhaaa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
// limitations under the License.

using System.Reflection;
using System.Text;

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
    public static IEnumerable<byte> Serialize<T>(T packet) where T : IPacket, new() => RawSerialize(packet);

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

            data.AddRange(typeof(IPacket).IsAssignableFrom(field.FieldType)
                ? RawSerialize(value)
                : GetDataFromField(value, field.FieldType));
        }

        return data;
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
        
        Chunk chunk = new Chunk(enumerable.ToList());

        object packet = new T();
        Type packetType = typeof(T);
        FieldInfo[] fields = packetType.GetFields();

        foreach (FieldInfo field in fields)
        {
            PacketFieldAttribute? attribute = field.GetCustomAttribute<PacketFieldAttribute>();
            if (attribute is null) continue;

            Type type = field.FieldType;

            object? value = typeof(IPacket).IsAssignableFrom(field.FieldType) ? RawDeserialize(chunk, type) : ValueFromByteArray(chunk, type);

            field.SetValue(packet, value);
        }

        return (T) packet;
    }

    private static object? RawDeserialize(Chunk chunk, Type packetType)
    {
        object? packet = Activator.CreateInstance(packetType);
        FieldInfo[] fields = packetType.GetFields();

        foreach (FieldInfo field in fields)
        {
            PacketFieldAttribute? attribute = field.GetCustomAttribute<PacketFieldAttribute>();
            if (attribute is null) continue;

            Type type = field.FieldType;

            object? value = ValueFromByteArray(chunk, type);

            field.SetValue(packet, value);
        }

        return packet;
    }

    private static byte[] GetDataFromField(object? value, Type type)
    {
        if (value is null) return new byte[] {0};

        if (type.IsArray && TypeSerializer.Serializers.ContainsKey(type.GetElementType()!))
        {
            Array? values = value as Array;

            if (values is null) return new byte[] {0};

            List<byte> data = new List<byte>(BitConverter.GetBytes(values.Length));
            
            foreach (object o in values) data.AddRange(TypeSerializer.Serializers[type.GetElementType()!].Invoke(o));
            
            return data.ToArray();
        }

        if (!TypeSerializer.Serializers.ContainsKey(type))
            return Array.Empty<byte>();
        
        return TypeSerializer.Serializers[type].Invoke(value);
    }

    private static object? ValueFromByteArray(Chunk data, Type type)
    {
        if (type.IsArray && TypeSerializer.Deserializers.ContainsKey(type.GetElementType()!))
        {
            int length = data.ReadInt();

            object[] values = new object[length];

            for (int i = 0; i < length; i++)
                values[i] = TypeSerializer.Deserializers[type.GetElementType()!].Invoke(data) ?? Array.Empty<byte>();

            Array newArray = Array.CreateInstance(type.GetElementType()!, length);
            Array.Copy(values, newArray, values.Length);
            
            return newArray;
        }
        
        if (!TypeSerializer.Deserializers.ContainsKey(type)) return null;

        return TypeSerializer.Deserializers[type].Invoke(data);
    }
}