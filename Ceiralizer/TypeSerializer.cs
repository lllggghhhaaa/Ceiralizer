// Copyright 2023 lllggghhhaaa
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

namespace Ceiralizer;

public static class TypeSerializer
{
    public static Dictionary<Type, Func<object, byte[]>> Serializers = new()
    {
        { typeof(bool), value => (bool)value ? new byte[] { 1 } : new byte[] { 0 } },
        { typeof(byte), value => new[] { (byte)value } },
        { typeof(sbyte), value => new[] { (byte)value } },
        { typeof(char), value => PacketSerializer.StringEncoder.GetBytes(((char)value).ToString()) },
        { typeof(double), value => BitConverter.GetBytes((double)value) },
        { typeof(float), value => BitConverter.GetBytes((float)value) },
        { typeof(int), value => BitConverter.GetBytes((int)value) },
        { typeof(uint), value => BitConverter.GetBytes((uint)value) },
        { typeof(long), value => BitConverter.GetBytes((long)value) },
        { typeof(ulong), value => BitConverter.GetBytes((ulong)value) },
        { typeof(short), value => BitConverter.GetBytes((short)value) },
        { typeof(ushort), value => BitConverter.GetBytes((ushort)value) },
        {
            typeof(string), value =>
            {
                string data = (string)value;
                List<byte> header =
                    new List<byte>(BitConverter.GetBytes(PacketSerializer.StringEncoder.GetByteCount(data)));
                header.AddRange(PacketSerializer.StringEncoder.GetBytes(data));

                return header.ToArray();
            }
        }
    };

    public static Dictionary<Type, Func<Chunk, object?>> Deserializers = new()
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