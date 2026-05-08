using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Ceiralizer.Utils;

public class ChunkWriter(ArrayBufferWriter<byte> writer)
{
    public byte[] GetWrittenData() => writer.WrittenSpan.ToArray();

    public void Write(ReadOnlySpan<byte> value)
        => writer.Write(value);
    
    public void Write(byte value)
    {
        var span = writer.GetSpan(sizeof(byte));
        span[0] = value;
        writer.Advance(sizeof(byte));
    }
    
    public void Write(sbyte value)
    {
        var span = writer.GetSpan(sizeof(sbyte));
        span[0] = (byte)value;
        writer.Advance(sizeof(sbyte));
    }
    
    public void Write(short value)
    {
        var span = writer.GetSpan(sizeof(short));
        BinaryPrimitives.WriteInt16LittleEndian(span, value);
        writer.Advance(sizeof(short));
    }
    
    public void Write(ushort value)
    {
        var span = writer.GetSpan(sizeof(ushort));
        BinaryPrimitives.WriteUInt16LittleEndian(span, value);
        writer.Advance(sizeof(ushort));
    }
    
    public void Write(int value)
    {
        var span = writer.GetSpan(sizeof(int));
        BinaryPrimitives.WriteInt32LittleEndian(span, value);
        writer.Advance(sizeof(int));
    }
    
    public void Write(uint value)
    {
        var span = writer.GetSpan(sizeof(uint));
        BinaryPrimitives.WriteUInt32LittleEndian(span, value);
        writer.Advance(sizeof(uint));
    }
    
    public void Write(long value)
    {
        var span = writer.GetSpan(sizeof(long));
        BinaryPrimitives.WriteInt64LittleEndian(span, value);
        writer.Advance(sizeof(long));
    }
    
    public void Write(ulong value)
    {
        var span = writer.GetSpan(sizeof(ulong));
        BinaryPrimitives.WriteUInt64LittleEndian(span, value);
        writer.Advance(sizeof(ulong));
    }

    public void Write(float value)
    {
        var span = writer.GetSpan(sizeof(float));
        BinaryPrimitives.WriteInt32LittleEndian(span, BitConverter.SingleToInt32Bits(value));
        writer.Advance(sizeof(float));
    }
    
    public void Write(double value)
    {
        var span = writer.GetSpan(sizeof(double));
        BinaryPrimitives.WriteInt64LittleEndian(span, BitConverter.DoubleToInt64Bits(value));
        writer.Advance(sizeof(double));
    }
    
    public void Write(bool value)
    {
        var span = writer.GetSpan(sizeof(bool));
        span[0] = value ? (byte)1 : (byte)0;
        writer.Advance(sizeof(bool));
    }
    
    public void Write(char value, Encoding encoder)
    {
        Span<char> chars = stackalloc char[1] { value };
        int size = encoder.GetByteCount(chars);
        var span = writer.GetSpan(size);
        var written = encoder.GetBytes(chars, span);
        writer.Advance(written);
    }

    public void Write(string value, Encoding encoder)
    {
        var size = encoder.GetByteCount(value);
        var span = writer.GetSpan(size);
        var written = encoder.GetBytes(value.AsSpan(), span);
        writer.Advance(written);
    }
}