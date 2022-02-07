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

using System.Text;
using Ceiralizer.Utils;

namespace Ceiralizer;

public class Chunk
{
    public List<byte> Data;
    public int Position;

    public Chunk(byte[] data) => Data = new List<byte>(data);

    public Chunk(List<byte> data) => Data = data;

    public bool ReadBool() => ReadByte() > 0;

    public byte ReadByte()
    {
        byte segment = Data[Position];
        Position++;

        return segment;
    }

    public sbyte ReadSByte()
    {
        sbyte segment = (sbyte) Data[Position];
        Position++;

        return segment;
    }

    public short ReadShort()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 2);
        Position += 2;

        return BitConverter.ToInt16(segment);
    }

    public ushort ReadUShort()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 2);
        Position += 2;

        return BitConverter.ToUInt16(segment);
    }
    
    public int ReadInt()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 4);
        Position += 4;

        return BitConverter.ToInt32(segment);
    }
    
    public uint ReadUInt()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 4);
        Position += 4;

        return BitConverter.ToUInt32(segment);
    }

    public long ReadLong()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToInt64(segment);
    }
    
    public ulong ReadULong()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToUInt64(segment);
    }
    
    public float ReadFloat()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToSingle(segment);
    }
    
    public double ReadDouble()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToDouble(segment);
    }

    public char ReadChar(Encoding encoder)
    {
        int size = encoder.GetByteCount(".");

        ArraySegment<byte> segment = Data.GetSegment(Position, size);
        Position += size;

        return encoder.GetString(segment).First();
    }

    public string ReadString(Encoding encoder, int lenght)
    {
        int size = encoder.GetByteCount(".") * lenght;

        ArraySegment<byte> segment = Data.GetSegment(Position, size);
        Position += size;

        return encoder.GetString(segment);
    }

    public ArraySegment<byte> ReadSegment(int lenght)
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, lenght);
        Position += lenght;

        return segment;
    }
}