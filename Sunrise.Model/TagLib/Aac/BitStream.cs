using System.Collections;

namespace Sunrise.Model.TagLib.Aac;

/// <summary>This class is used to help reading arbitary number of bits from a fixed array of bytes
/// </summary>
public class BitStream
{
    private readonly BitArray _bits;
    private int _bitindex;

    /// <summary>Construct a new <see cref="BitStream"/></summary>
    /// <param name="buffer">A <see cref="Byte[]"/>, must be 7 bytes long</param>
    public BitStream(byte[] buffer)
    {
        if (buffer.Length != 7)
            throw new ArgumentException("Buffer size must be 7 bytes");

        _bits = new BitArray(buffer.Length * 8); // Reverse bits  

        for (int i = 0; i < buffer.Length; i++)
        {
            for (int y = 0; y < 8; y++)
                _bits[i * 8 + y] = (buffer[i] & (1 << (7 - y))) > 0;
        }

        _bitindex = 0;
    }

    /// <summary>Reads an Int32 from the bitstream</summary>
    /// <param name="numberOfBits">A <see cref="int" /> value containing the number of bits to read from the bitstream</param>
    public int ReadInt32(int numberOfBits)
    {
        if (numberOfBits <= 0)
            throw new ArgumentException("Number of bits to read must be >= 1");

        if (numberOfBits > 32)
            throw new ArgumentException("Number of bits to read must be <= 32");

        int value = 0;
        int start = _bitindex + numberOfBits - 1;
        for (int i = 0; i < numberOfBits; i++)
        {
            value += _bits[start] ? (1 << i) : 0;
            _bitindex++;
            start--;
        }

        return value;
    }

}
