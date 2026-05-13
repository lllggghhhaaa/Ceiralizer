using System.Buffers.Binary;
using System.Text;

namespace Ceiralizer.Utils;

public class ChunkReader
{
    private readonly ReadOnlyMemory<byte> _data;
    private int _position;

    public int Position => _position;
    public int Remaining => _data.Length - _position;
    public bool CanRead(int length = 1) => _position + length <= _data.Length;

    public ChunkReader(ReadOnlyMemory<byte> data) => _data = data;
    public ChunkReader(byte[] data) => _data = data;

    private ReadOnlySpan<byte> Consume(int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Read length cannot be negative.");

        if (!CanRead(length))
            throw new InvalidOperationException(
                $"Cannot read {length} byte(s) from position {_position}. Remaining bytes: {Remaining}.");

        var span = _data.Span.Slice(_position, length);
        _position += length;
        return span;
    }

    public bool ReadBool() => ReadByte() > 0;
    public byte ReadByte() => Consume(sizeof(byte))[0];
    public sbyte ReadSByte() => (sbyte)Consume(sizeof(sbyte))[0];
    public short ReadShort() => BinaryPrimitives.ReadInt16LittleEndian(Consume(sizeof(short)));
    public ushort ReadUShort() => BinaryPrimitives.ReadUInt16LittleEndian(Consume(sizeof(ushort)));
    public int ReadInt() => BinaryPrimitives.ReadInt32LittleEndian(Consume(sizeof(int)));
    public uint ReadUInt() => BinaryPrimitives.ReadUInt32LittleEndian(Consume(sizeof(uint)));
    public long ReadLong() => BinaryPrimitives.ReadInt64LittleEndian(Consume(sizeof(long)));
    public ulong ReadULong() => BinaryPrimitives.ReadUInt64LittleEndian(Consume(sizeof(ulong)));

    public float ReadFloat() =>
        BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(Consume(sizeof(float))));

    public double ReadDouble() =>
        BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(Consume(sizeof(double))));

    public string ReadString(Encoding encoder, int byteCount) =>
        encoder.GetString(Consume(byteCount));

    public char ReadChar(Encoding encoder)
    {
        var b0 = Consume(1)[0];

        var byteCount =
            b0 < 0x80 ? 1 :
            b0 < 0xE0 ? 2 :
            b0 < 0xF0 ? 3 : 4;

        Span<byte> bytes = stackalloc byte[byteCount];
        bytes[0] = b0;

        if (byteCount > 1)
            Consume(byteCount - 1).CopyTo(bytes[1..]);

        Span<char> chars = stackalloc char[2];
        var count = Encoding.UTF8.GetChars(bytes, chars);

        return count == 1 ? chars[0] : throw new InvalidOperationException("Char requires surrogate pair.");
    }

    public ReadOnlySpan<byte> ReadSegment(int length) => Consume(length);

    public void ResetPosition(int pos = 0) => _position = pos;
}