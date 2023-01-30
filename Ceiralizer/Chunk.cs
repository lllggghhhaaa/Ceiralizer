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

public class Chunk : IDisposable
{
    public List<byte> Data;
    public int Position;

    private bool _disposed;

    /// <summary>
    /// Initialize a empty chunk
    /// </summary>
    public Chunk() => Data = new List<byte>();
    /// <summary>
    /// Initialize from byte array
    /// </summary>
    /// <param name="data">byte array with data</param>
    public Chunk(byte[] data) => Data = new List<byte>(data);
    /// <summary>
    /// Initialize from byte list
    /// </summary>
    /// <param name="data">byte list with data</param>
    public Chunk(List<byte> data) => Data = data;
    
    /// <summary>
    /// Check if is possible read more data
    /// </summary>
    /// <returns>Position + len is less than Count</returns>
    public bool CanRead(int len = 0) => Position + len < Data.Count;
    
    /// <summary>
    /// Get the data as an array
    /// </summary>
    /// <returns></returns>
    public byte[] ToArray() => Data.ToArray();
    
    /// <summary>
    /// Get remain lenght
    /// </summary>
    /// <returns>Count - Position</returns>
    public int RemainLenght() => Data.Count - Position;

    
    /// <summary>
    /// Read an byte as boolean
    /// </summary>
    /// <returns>boolean in this position</returns>
    public bool ReadBool() => ReadByte() > 0;

    /// <summary>
    /// Write an boolean to data
    /// </summary>
    /// <param name="value">boolean value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(bool value, int? pos = null)
    {
        byte data = BitConverter.GetBytes(value)[0];

        if (pos is null) Data.Add(data);
        else Data.Insert(pos.Value, data);
    }
    

    /// <summary>
    /// Read an byte
    /// </summary>
    /// <returns>byte in this position</returns>
    public byte ReadByte()
    {
        byte segment = Data[Position];
        Position++;

        return segment;
    }

    /// <summary>
    /// Write an byte to data
    /// </summary>
    /// <param name="value">byte value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(byte value, int? pos = null)
    {
        if (pos is null) Data.Add(value);
        else Data.Insert(pos.Value, value);
    }
    

    /// <summary>
    /// Read an byte as an signed byte
    /// </summary>
    /// <returns>sbyte in this position</returns>
    public sbyte ReadSByte()
    {
        sbyte segment = (sbyte) Data[Position];
        Position++;

        return segment;
    }

    /// <summary>
    /// Write an signed byte to data
    /// </summary>
    /// <param name="value">sbyte value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(sbyte value, int? pos = null)
    {
        if (pos is null) Data.Add((byte) value);
        else Data.Insert(pos.Value, (byte) value);
    }
    

    /// <summary>
    /// Read an short
    /// </summary>
    /// <returns>short in this position</returns>
    public short ReadShort()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 2);
        Position += 2;

        return BitConverter.ToInt16(segment);
    }

    /// <summary>
    /// Write an short to data
    /// </summary>
    /// <param name="value">short value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(short value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    

    /// <summary>
    /// Read an unsigned short
    /// </summary>
    /// <returns>ushort in this position</returns>
    public ushort ReadUShort()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 2);
        Position += 2;

        return BitConverter.ToUInt16(segment);
    }

    /// <summary>
    /// Write an unsigned short to data
    /// </summary>
    /// <param name="value">ushort value</param>
    /// <param name="pos">position in the data list</param>
    public void Write(ushort value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    
    
    /// <summary>
    /// Read an integer
    /// </summary>
    /// <returns>int in this position</returns>
    public int ReadInt()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 4);
        Position += 4;

        return BitConverter.ToInt32(segment);
    }
    
    /// <summary>
    /// Write an integer to data
    /// </summary>
    /// <param name="value">int value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(int value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    
    
    /// <summary>
    /// Read an unsigned integer
    /// </summary>
    /// <returns>uint in this position</returns>
    public uint ReadUInt()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 4);
        Position += 4;

        return BitConverter.ToUInt32(segment);
    }
    
    /// <summary>
    /// Write an unsigned integer to data
    /// </summary>
    /// <param name="value">uint value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(uint value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    

    /// <summary>
    /// Read an long
    /// </summary>
    /// <returns>long in this position</returns>
    public long ReadLong()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToInt64(segment);
    }
    
    /// <summary>
    /// Write an long to data
    /// </summary>
    /// <param name="value">long value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(long value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    
    
    /// <summary>
    /// Read an ulong
    /// </summary>
    /// <returns>ulong in this position</returns>
    public ulong ReadULong()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToUInt64(segment);
    }
    
    /// <summary>
    /// Write an ulong to data
    /// </summary>
    /// <param name="value">ulong value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(ulong value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    
    
    /// <summary>
    /// Read an float
    /// </summary>
    /// <returns>float in this position</returns>
    public float ReadFloat()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 4);
        Position += 4;

        return BitConverter.ToSingle(segment);
    }
    
    /// <summary>
    /// Write an float to data
    /// </summary>
    /// <param name="value">float value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(float value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    
    
    /// <summary>
    /// Read an double
    /// </summary>
    /// <returns>double in this position</returns>
    public double ReadDouble()
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, 8);
        Position += 8;

        return BitConverter.ToDouble(segment);
    }
    
    /// <summary>
    /// Write an double to data
    /// </summary>
    /// <param name="value">double value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(double value, int? pos = null)
    {
        if (pos is null) Data.AddRange(BitConverter.GetBytes(value));
        else Data.InsertRange(pos.Value, BitConverter.GetBytes(value));
    }
    

    /// <summary>
    /// Read an char
    /// </summary>
    /// <param name="encoder">String encoder (ASCII, UTF8, UNICODE...)</param>
    /// <returns>char in this position</returns>
    public char ReadChar(Encoding encoder)
    {
        int size = encoder.GetByteCount(".");

        ArraySegment<byte> segment = Data.GetSegment(Position, size);
        Position += size;

        return encoder.GetString(segment).First();
    }
    
    /// <summary>
    /// Write an char to data
    /// </summary>
    /// <param name="value">char value</param>
    /// <param name="encoder">String encoder (ASCII, UTF8, UNICODE...)</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(char value, Encoding encoder, int? pos = null)
    {
        if (pos is null) Data.AddRange(encoder.GetBytes(value.ToString()));
        else Data.InsertRange(pos.Value, encoder.GetBytes(value.ToString()));
    }
    

    /// <summary>
    /// Read an string
    /// </summary>
    /// <param name="encoder">String encoder (ASCII, UTF8, UNICODE...)</param>
    /// <param name="lenght">String lenght to read</param>
    /// <returns>string in this position with this lenght</returns>
    public string ReadString(Encoding encoder, int lenght)
    {
        int size = encoder.GetByteCount(".") * lenght;

        ArraySegment<byte> segment = Data.GetSegment(Position, size);
        Position += size;

        return encoder.GetString(segment);
    }
    
    /// <summary>
    /// Write an string to data
    /// </summary>
    /// <param name="value">string value</param>
    /// <param name="encoder">String encoder (ASCII, UTF8, UNICODE...)</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(string value, Encoding encoder, int? pos = null)
    {
        if (pos is null) Data.AddRange(encoder.GetBytes(value));
        else Data.InsertRange(pos.Value, encoder.GetBytes(value));
    }
    

    /// <summary>
    /// Read an byte ArraySegment
    /// </summary>
    /// <param name="lenght">Segment lenght</param>
    /// <returns>byte segment with lenght</returns>
    public ArraySegment<byte> ReadSegment(int lenght)
    {
        ArraySegment<byte> segment = Data.GetSegment(Position, lenght);
        Position += lenght;

        return segment;
    }

    /// <summary>
    /// Write an byte array to data
    /// </summary>
    /// <param name="value">byte array value</param>
    /// <param name="pos">Position in the data list</param>
    public void Write(byte[] value, int? pos)
    {
        if (pos is null) Data.AddRange(value);
        else Data.InsertRange(pos.Value, value);
    }
    

    /// <summary>
    /// Reset the chunk position
    /// </summary>
    /// <param name="pos">Start position</param>
    public void ResetPosition(int pos = 0) => Position = pos;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing) Data = null!;

        _disposed = true;
    }
}