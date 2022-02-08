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

    public void Write(bool value, int? pos = null)
    {
        byte data = BitConverter.GetBytes(value)[0];

        if (pos is null) Data.Add(data);
        else Data.Insert(pos.Value, data);
    }
    

    public byte ReadByte()
    {
        byte segment = Data[Position];
        Position++;

        return segment;
    }

    public void Write(byte value, int? pos = null)
    {
        if (pos is null) Data.Add(value);
        else Data.Insert(pos.Value, value);
    }
    

    public sbyte ReadSByte()
    {
        sbyte segment = (sbyte) Data[Position];
        Position++;

        return segment;
    }

    public void Write(sbyte value, int? pos = null)
    {
        if (pos is null) Data.Add((byte) value);
        else Data.Insert(pos.Value, (byte) value);
    }
    

    public short ReadShort()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 2);
        Position += 2;

        return BitConverter.ToInt16(segment);
    }

    public void Write(short value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    

    public ushort ReadUShort()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 2);
        Position += 2;

        return BitConverter.ToUInt16(segment);
    }

    public void Write(ushort value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    
    
    public int ReadInt()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 4);
        Position += 4;

        return BitConverter.ToInt32(segment);
    }
    
    public void Write(int value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    
    
    public uint ReadUInt()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 4);
        Position += 4;

        return BitConverter.ToUInt32(segment);
    }
    
    public void Write(uint value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    

    public long ReadLong()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToInt64(segment);
    }
    
    public void Write(long value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    
    
    public ulong ReadULong()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToUInt64(segment);
    }
    
    public void Write(ulong value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    
    
    public float ReadFloat()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToSingle(segment);
    }
    
    public void Write(float value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    
    
    public double ReadDouble()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToDouble(segment);
    }
    
    public void Write(double value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    

    public char ReadChar(Encoding encoder)
    {
        int size = encoder.GetByteCount(".");

        ArraySegment<byte> segment = Data.GetSegment(Position, size);
        Position += size;

        return encoder.GetString(segment).First();
    }
    
    public void Write(char value, Encoding encoder, int? pos = null)
    {
        if (pos is null) Data.AddRange(encoder.GetBytes(value.ToString()));
        else Data.InsertRange(pos.Value, encoder.GetBytes(value.ToString()));
    }
    

    public string ReadString(Encoding encoder, int lenght)
    {
        int size = encoder.GetByteCount(".") * lenght;

        ArraySegment<byte> segment = Data.GetSegment(Position, size);
        Position += size;

        return encoder.GetString(segment);
    }
    
    public void Write(string value, Encoding encoder, int? pos = null)
    {
        if (pos is null) Data.AddRange(encoder.GetBytes(value));
        else Data.InsertRange(pos.Value, encoder.GetBytes(value));
    }
    

    public ArraySegment<byte> ReadSegment(int lenght)
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, lenght);
        Position += lenght;

        return segment;
    }

    public void Write(byte[] value, int? pos)
    {
        if (pos is null) Data.AddRange(value);
        else Data.InsertRange(pos.Value, value);
    }
    

    public void ResetPosition(int pos = 0) => Position = pos;
}