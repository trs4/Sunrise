using System.Collections;
using System.Text;

namespace Sunrise.Model.TagLib;

public class ByteVector : IList<byte>, IComparable<ByteVector>
{
#pragma warning disable IDE0300 // Simplify collection initialization
    private static readonly uint[] _crcTable = new uint[256] {
#pragma warning restore IDE0300 // Simplify collection initialization
            0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9,
            0x130476dc, 0x17c56b6b, 0x1a864db2, 0x1e475005,
            0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61,
            0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd,
            0x4c11db70, 0x48d0c6c7, 0x4593e01e, 0x4152fda9,
            0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
            0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011,
            0x791d4014, 0x7ddc5da3, 0x709f7b7a, 0x745e66cd,
            0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039,
            0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5,
            0xbe2b5b58, 0xbaea46ef, 0xb7a96036, 0xb3687d81,
            0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
            0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49,
            0xc7361b4c, 0xc3f706fb, 0xceb42022, 0xca753d95,
            0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1,
            0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d,
            0x34867077, 0x30476dc0, 0x3d044b19, 0x39c556ae,
            0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
            0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16,
            0x018aeb13, 0x054bf6a4, 0x0808d07d, 0x0cc9cdca,
            0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde,
            0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02,
            0x5e9f46bf, 0x5a5e5b08, 0x571d7dd1, 0x53dc6066,
            0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
            0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e,
            0xbfa1b04b, 0xbb60adfc, 0xb6238b25, 0xb2e29692,
            0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6,
            0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a,
            0xe0b41de7, 0xe4750050, 0xe9362689, 0xedf73b3e,
            0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
            0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686,
            0xd5b88683, 0xd1799b34, 0xdc3abded, 0xd8fba05a,
            0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637,
            0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb,
            0x4f040d56, 0x4bc510e1, 0x46863638, 0x42472b8f,
            0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
            0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47,
            0x36194d42, 0x32d850f5, 0x3f9b762c, 0x3b5a6b9b,
            0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff,
            0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623,
            0xf12f560e, 0xf5ee4bb9, 0xf8ad6d60, 0xfc6c70d7,
            0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
            0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f,
            0xc423cd6a, 0xc0e2d0dd, 0xcda1f604, 0xc960ebb3,
            0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7,
            0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b,
            0x9b3660c6, 0x9ff77d71, 0x92b45ba8, 0x9675461f,
            0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
            0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640,
            0x4e8ee645, 0x4a4ffbf2, 0x470cdd2b, 0x43cdc09c,
            0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8,
            0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24,
            0x119b4be9, 0x155a565e, 0x18197087, 0x1cd86d30,
            0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
            0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088,
            0x2497d08d, 0x2056cd3a, 0x2d15ebe3, 0x29d4f654,
            0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0,
            0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c,
            0xe3a1cbc1, 0xe760d676, 0xea23f0af, 0xeee2ed18,
            0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
            0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0,
            0x9abc8bd5, 0x9e7d9662, 0x933eb0bb, 0x97ffad0c,
            0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668,
            0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4
        };
    private static readonly ReadOnlyByteVector _td1 = new ReadOnlyByteVector(1);
    private static readonly ReadOnlyByteVector _td2 = new ReadOnlyByteVector(2);
    private static readonly Encoding _defaultEncoding = Encoding.GetEncoding(1251);
    private static Encoding _lastUtf16Encoding = Encoding.Unicode;
    private readonly List<byte> _data = [];

    public ByteVector() { }

    public ByteVector(ByteVector vector)
    {
        if (vector is not null)
            _data.AddRange(vector);
    }

    public ByteVector(params byte[] data)
    {
        if (data is not null)
            _data.AddRange(data);
    }

    public ByteVector(byte[] data, int length)
    {
        if (length > data.Length)
            throw new ArgumentOutOfRangeException(nameof(length), "Length exceeds size of data");

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length is less than zero");

        if (length == data.Length)
            _data.AddRange(data);
        else
        {
            byte[] array = new byte[length];
            Array.Copy(data, 0, array, 0, length);
            _data.AddRange(array);
        }
    }

    public ByteVector(int size) : this(size, 0) { }

    public ByteVector(int size, byte value)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size is less than zero");

        if (size == 0)
            return;

        byte[] data = new byte[size];

        for (int i = 0; i < size; i++)
            data[i] = value;

        _data.AddRange(data);
    }

    public byte[] Data => [.. _data];

    public bool IsEmpty => _data.Count == 0;

    public uint Checksum
    {
        get
        {
            uint sum = 0;

            foreach (byte b in _data)
                sum = (sum << 8) ^ _crcTable[((sum >> 24) & 0xFF) ^ b];

            return sum;
        }
    }

    public static bool UseBrokenLatin1Behavior { get; set; }

    public ByteVector Mid(int startIndex, int length)
    {
        if (startIndex < 0 || startIndex > Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        if (length < 0 || startIndex + length > Count)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (length == 0)
            return [];

        if (startIndex + length > _data.Count)
            length = _data.Count - startIndex;

        byte[] data = new byte[length];
        _data.CopyTo(startIndex, data, 0, length);
        return data;
    }

    public ByteVector Mid(int index) => Mid(index, Count - index);

    public int Find(ByteVector pattern, int offset, int byteAlign)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "offset must be at least 0");

        if (byteAlign < 1)
            throw new ArgumentOutOfRangeException(nameof(byteAlign), "byteAlign must be at least 1");

        if (pattern.Count > Count - offset)
            return -1;

        if (pattern.Count == 1)
        {
            byte p = pattern[0];

            for (int i = offset; i < _data.Count; i += byteAlign)
            {
                if (_data[i] == p)
                    return i;
            }

            return -1;
        }

        int[] last_occurrence = new int[256];

        for (int i = 0; i < 256; ++i)
            last_occurrence[i] = pattern.Count;

        for (int i = 0; i < pattern.Count - 1; ++i)
            last_occurrence[pattern[i]] = pattern.Count - i - 1;

        for (int i = pattern.Count - 1 + offset; i < _data.Count; i += last_occurrence[_data[i]])
        {
            int iBuffer = i;
            int iPattern = pattern.Count - 1;

            while (iPattern >= 0 && _data[iBuffer] == pattern[iPattern])
            {
                --iBuffer;
                --iPattern;
            }

            if (-1 == iPattern && (iBuffer + 1 - offset) % byteAlign == 0)
                return iBuffer + 1;
        }

        return -1;
    }

    public int Find(ByteVector pattern, int offset) => Find(pattern, offset, 1);

    public int Find(ByteVector pattern) => Find(pattern, 0, 1);

    public int RFind(ByteVector pattern, int offset, int byteAlign)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        if (pattern.Count == 0 || pattern.Count > Count - offset)
            return -1;

        if (pattern.Count == 1)
        {
            byte p = pattern[0];

            for (int i = Count - offset - 1; i >= 0; i -= byteAlign)
            {
                if (_data[i] == p)
                    return i;
            }

            return -1;
        }

        int[] first_occurrence = new int[256];

        for (int i = 0; i < 256; ++i)
            first_occurrence[i] = pattern.Count;

        for (int i = pattern.Count - 1; i > 0; --i)
            first_occurrence[pattern[i]] = i;

        for (int i = Count - offset - pattern.Count; i >= 0; i -= first_occurrence[_data[i]])
        {
            if ((offset - i) % byteAlign == 0 && ContainsAt(pattern, i))
                return i;
        }

        return -1;
    }

    public int RFind(ByteVector pattern, int offset) => RFind(pattern, offset, 1);

    public int RFind(ByteVector pattern) => RFind(pattern, 0, 1);

    public bool ContainsAt(ByteVector pattern, int offset, int patternOffset, int patternLength)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern.Count < patternLength)
            patternLength = pattern.Count;

        if (patternLength > _data.Count || offset >= _data.Count || patternOffset >= pattern.Count || patternLength <= 0 || offset < 0)
            return false;

        for (int i = 0; i < patternLength - patternOffset; i++)
        {
            if (_data[i + offset] != pattern[i + patternOffset])
                return false;
        }

        return true;
    }

    public bool ContainsAt(ByteVector pattern, int offset, int patternOffset) => ContainsAt(pattern, offset, patternOffset, int.MaxValue);

    public bool ContainsAt(ByteVector pattern, int offset) => ContainsAt(pattern, offset, 0);

    public bool StartsWith(ByteVector pattern) => ContainsAt(pattern, 0);

    public bool EndsWith(ByteVector pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return ContainsAt(pattern, _data.Count - pattern.Count);
    }

    public int EndsWithPartialMatch(ByteVector pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern.Count > _data.Count)
            return -1;

        int start_index = _data.Count - pattern.Count;

        for (int i = 1; i < pattern.Count; i++)
        {
            if (ContainsAt(pattern, start_index + i, 0, pattern.Count - i))
                return start_index + i;
        }

        return -1;
    }

    public void Add(ByteVector data)
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects");

        if (data is not null)
            _data.AddRange(data);
    }

    public void Add(byte[] data)
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects");

        if (data is not null)
            _data.AddRange(data);
    }

    public void Insert(int index, ByteVector data)
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects");

        if (data is not null)
            _data.InsertRange(index, data);
    }

    public void Insert(int index, byte[] data)
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects.");

        if (data is not null)
            _data.InsertRange(index, data);
    }

    public ByteVector Resize(int size, byte padding)
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects.");

        if (_data.Count > size)
            _data.RemoveRange(size, _data.Count - size);

        while (_data.Count < size)
            _data.Add(padding);

        return this;
    }

    public ByteVector Resize(int size) => Resize(size, 0);

    public void RemoveRange(int index, int count)
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects.");

        _data.RemoveRange(index, count);
    }

    public int ToInt(bool mostSignificantByteFirst)
    {
        int sum = 0;
        int last = Count > 4 ? 3 : Count - 1;

        for (int i = 0; i <= last; i++)
        {
            int offset = mostSignificantByteFirst ? last - i : i;

            unchecked
            {
                sum |= this[i] << (offset * 8);
            }
        }

        return sum;
    }

    public int ToInt() => ToInt(true);

    public uint ToUInt(bool mostSignificantByteFirst)
    {
        uint sum = 0;
        int last = Count > 4 ? 3 : Count - 1;

        for (int i = 0; i <= last; i++)
        {
            int offset = mostSignificantByteFirst ? last - i : i;
            sum |= (uint)this[i] << (offset * 8);
        }

        return sum;
    }

    public uint ToUInt() => ToUInt(true);

    public short ToShort(bool mostSignificantByteFirst)
    {
        short sum = 0;
        int last = Count > 2 ? 1 : Count - 1;

        for (int i = 0; i <= last; i++)
        {
            int offset = mostSignificantByteFirst ? last - i : i;

            unchecked
            {
                sum |= (short)(this[i] << (offset * 8));
            }
        }

        return sum;
    }

    public short ToShort() => ToShort(true);

    public ushort ToUShort(bool mostSignificantByteFirst)
    {
        ushort sum = 0;
        int last = Count > 2 ? 1 : Count - 1;

        for (int i = 0; i <= last; i++)
        {
            int offset = mostSignificantByteFirst ? last - i : i;
            sum |= (ushort)(this[i] << (offset * 8));
        }

        return sum;
    }

    public ushort ToUShort() => ToUShort(true);

    public long ToLong(bool mostSignificantByteFirst)
    {
        long sum = 0;
        int last = Count > 8 ? 7 : Count - 1;

        for (int i = 0; i <= last; i++)
        {
            int offset = mostSignificantByteFirst ? last - i : i;

            unchecked
            {
                sum |= (long)this[i] << (offset * 8);
            }
        }

        return sum;
    }

    public long ToLong() => ToLong(true);

    public ulong ToULong(bool mostSignificantByteFirst)
    {
        ulong sum = 0;
        int last = Count > 8 ? 7 : Count - 1;

        for (int i = 0; i <= last; i++)
        {
            int offset = mostSignificantByteFirst ? last - i : i;
            sum |= (ulong)this[i] << (offset * 8);
        }

        return sum;
    }

    public ulong ToULong() => ToULong(true);

    public float ToFloat(bool mostSignificantByteFirst)
    {
        byte[] bytes = (byte[])Data.Clone();

        if (mostSignificantByteFirst)
            Array.Reverse(bytes);

        return BitConverter.ToSingle(bytes, 0);
    }

    public float ToFloat() => ToFloat(true);

    public double ToDouble(bool mostSignificantByteFirst)
    {
        byte[] bytes = (byte[])Data.Clone();

        if (mostSignificantByteFirst)
            Array.Reverse(bytes);

        return BitConverter.ToDouble(bytes, 0);
    }

    public double ToDouble() => ToDouble(true);

    public double ToExtendedPrecision()
    {
        int exponent = ((this[0] & 0x7F) << 8) | this[1];

        ulong hiMantissa = ((ulong)this[2] << 24)
                           | ((ulong)this[3] << 16)
                           | ((ulong)this[4] << 8)
                           | this[5];

        ulong loMantissa = ((ulong)this[6] << 24)
                           | ((ulong)this[7] << 16)
                           | ((ulong)this[8] << 8)
                           | this[9];

        double f;

        if (exponent == 0 && hiMantissa == 0 && loMantissa == 0)
            f = 0;
        else if (exponent == 0x7FFF)
            f = double.PositiveInfinity;
        else
        {
            exponent -= 16383;
            f = hiMantissa * Math.Pow(2, exponent -= 31);
            f += loMantissa * Math.Pow(2, exponent -= 32);
        }

        return (this[0] & 0x80) != 0 ? -f : f;
    }

    public string ToString(StringType type, int offset, int count)
    {
        if (offset < 0 || offset > Count)
            throw new ArgumentOutOfRangeException(nameof(offset));

        if (count < 0 || count + offset > Count)
            throw new ArgumentOutOfRangeException(nameof(count));

        var bom = type == StringType.UTF16 && _data.Count - offset > 1 ? Mid(offset, 2) : null;
        string s = StringTypeToEncoding(type, bom).GetString(Data, offset, count);

        if (s.Length != 0 && (s[0] == 0xfffe || s[0] == 0xfeff)) // UTF16 BOM
            return s.Substring(1);

        return s;
    }

    public string ToString(StringType type) => ToString(type, 0, Count);

    public override string ToString() => ToString(StringType.UTF8, 0, Count);

    public string[] ToStrings(StringType type, int offset) => ToStrings(type, offset, int.MaxValue);

    public string[] ToStrings(StringType type, int offset, int count)
    {
        int chunk = 0;
        int position = offset;
        var list = new List<string>();
        var separator = TextDelimiter(type);
        int align = separator.Count;

        while (chunk < count && position < Count)
        {
            int start = position;

            if (chunk + 1 == count)
                position = offset + count;
            else
            {
                position = Find(separator, start, align);

                if (position < 0)
                    position = Count;
            }

            int length = position - start;

            if (length == 0)
                list.Add(string.Empty);
            else
            {
                string s = ToString(type, start, length);

                if (s.Length != 0 && (s[0] == 0xfffe || s[0] == 0xfeff)) // UTF16 BOM
                    s = s.Substring(1);

                list.Add(s);
            }

            position += align;
        }

        return [.. list];
    }

    public static bool operator ==(ByteVector first, ByteVector second)
    {
        if (first is null && second is null)
            return true;
        else if (first is null || second is null)
            return false;

        return first.Equals(second);
    }

    public static bool operator !=(ByteVector first, ByteVector second) => !(first == second);

    public static bool operator <(ByteVector first, ByteVector second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        return first.CompareTo(second) < 0;
    }

    public static bool operator <=(ByteVector first, ByteVector second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        return first.CompareTo(second) <= 0;
    }

    public static bool operator >(ByteVector first, ByteVector second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        return first.CompareTo(second) > 0;
    }

    public static bool operator >=(ByteVector first, ByteVector second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        return first.CompareTo(second) >= 0;
    }

    public static ByteVector operator +(ByteVector first, ByteVector second) => new ByteVector(first) { second };

    public static implicit operator ByteVector(byte value) => new ByteVector(value);

    public static implicit operator ByteVector(byte[] value) => new ByteVector(value);

    public static implicit operator ByteVector(string value) => FromString(value, StringType.UTF8);

    public static ByteVector FromInt(int value, bool mostSignificantByteFirst)
    {
        var vector = new ByteVector();

        for (int i = 0; i < 4; i++)
        {
            int offset = mostSignificantByteFirst ? 3 - i : i;
            vector.Add((byte)(value >> (offset * 8) & 0xFF));
        }

        return vector;
    }

    public static ByteVector FromInt(int value) => FromInt(value, true);

    public static ByteVector FromUInt(uint value, bool mostSignificantByteFirst)
    {
        var vector = new ByteVector();

        for (int i = 0; i < 4; i++)
        {
            int offset = mostSignificantByteFirst ? 3 - i : i;
            vector.Add((byte)(value >> (offset * 8) & 0xFF));
        }

        return vector;
    }

    public static ByteVector FromUInt(uint value) => FromUInt(value, true);

    public static ByteVector FromShort(short value, bool mostSignificantByteFirst)
    {
        var vector = new ByteVector();

        for (int i = 0; i < 2; i++)
        {
            int offset = mostSignificantByteFirst ? 1 - i : i;
            vector.Add((byte)(value >> (offset * 8) & 0xFF));
        }

        return vector;
    }

    public static ByteVector FromShort(short value) => FromShort(value, true);

    public static ByteVector FromUShort(ushort value, bool mostSignificantByteFirst)
    {
        var vector = new ByteVector();

        for (int i = 0; i < 2; i++)
        {
            int offset = mostSignificantByteFirst ? 1 - i : i;
            vector.Add((byte)(value >> (offset * 8) & 0xFF));
        }

        return vector;
    }

    public static ByteVector FromUShort(ushort value) => FromUShort(value, true);

    public static ByteVector FromLong(long value, bool mostSignificantByteFirst)
    {
        var vector = new ByteVector();

        for (int i = 0; i < 8; i++)
        {
            int offset = mostSignificantByteFirst ? 7 - i : i;
            vector.Add((byte)(value >> (offset * 8) & 0xFF));
        }

        return vector;
    }

    public static ByteVector FromLong(long value) => FromLong(value, true);

    public static ByteVector FromULong(ulong value, bool mostSignificantByteFirst)
    {
        var vector = new ByteVector();

        for (int i = 0; i < 8; i++)
        {
            int offset = mostSignificantByteFirst ? 7 - i : i;
            vector.Add((byte)(value >> (offset * 8) & 0xFF));
        }

        return vector;
    }

    public static ByteVector FromULong(ulong value) => FromULong(value, true);

    public static ByteVector FromString(string text, StringType type, int length)
    {
        var data = new ByteVector();

        if (type == StringType.UTF16)
            data.Add([0xff, 0xfe]);

        if (text is null || text.Length == 0)
            return data;

        if (text.Length > length)
            text = text.Substring(0, length);

        data.Add(StringTypeToEncoding(type, data).GetBytes(text));
        return data;
    }

    public static ByteVector FromString(string text, StringType type) => FromString(text, type, int.MaxValue);

    public static ByteVector FromString(string text, int length) => FromString(text, StringType.UTF8, length);

    public static ByteVector FromPath(string path) => FromPath(path, out _, false);

    internal static ByteVector FromPath(string path, out byte[]? firstChunk, bool copyFirstChunk)
    {
        ArgumentNullException.ThrowIfNull(path);
        return FromFile(new File.LocalFileAbstraction(path), out firstChunk, copyFirstChunk);
    }

    public static ByteVector FromFile(File.IFileAbstraction abstraction) => FromFile(abstraction, out _, false);

    internal static ByteVector FromFile(File.IFileAbstraction abstraction, out byte[]? firstChunk, bool copyFirstChunk)
    {
        ArgumentNullException.ThrowIfNull(abstraction);
        var stream = abstraction.ReadStream;
        var output = FromStream(stream, out firstChunk, copyFirstChunk);
        abstraction.CloseStream(stream);
        return output;
    }

    public static ByteVector FromStream(Stream stream) => FromStream(stream, out _, false);

    internal static ByteVector FromStream(Stream stream, out byte[]? firstChunk, bool copyFirstChunk)
    {
        var vector = new ByteVector();
        byte[] bytes = new byte[4096];
        int read_size = bytes.Length;
        int bytes_read = 0;
        bool set_first_chunk = false;
        firstChunk = null;

        while (true)
        {
            Array.Clear(bytes, 0, bytes.Length);
            int n = stream.Read(bytes, 0, read_size);
            vector.Add(bytes);
            bytes_read += n;

            if (!set_first_chunk)
            {
                if (copyFirstChunk)
                {
                    if (firstChunk is null || firstChunk.Length != read_size)
                        firstChunk = new byte[read_size];

                    Array.Copy(bytes, 0, firstChunk, 0, n);
                }
                set_first_chunk = true;
            }

            if ((bytes_read == stream.Length && stream.Length > 0) || (n < read_size && stream.Length <= 0))
                break;
        }

        if (stream.Length > 0 && vector.Count != stream.Length)
            vector.Resize((int)stream.Length);

        return vector;
    }

    public static ByteVector TextDelimiter(StringType type) => type is StringType.UTF16 or StringType.UTF16BE or StringType.UTF16LE ? _td2 : _td1;

    private static Encoding StringTypeToEncoding(StringType type, ByteVector bom)
    {
        switch (type)
        {
            case StringType.UTF16:
                if (bom is null)
                    return _lastUtf16Encoding;

                if (bom[0] == 0xFF && bom[1] == 0xFE)
                    return _lastUtf16Encoding = Encoding.Unicode;

                if (bom[1] == 0xFF && bom[0] == 0xFE)
                    return _lastUtf16Encoding = Encoding.BigEndianUnicode;

                return _lastUtf16Encoding;
            case StringType.UTF16BE:
                return Encoding.BigEndianUnicode;
            case StringType.UTF8:
                return Encoding.UTF8;
            case StringType.UTF16LE:
                return Encoding.Unicode;
        }

        if (UseBrokenLatin1Behavior)
            return Encoding.Default;

        return _defaultEncoding;
    }

    public override bool Equals(object? other) => other is ByteVector bv && Equals(bv);

    public bool Equals(ByteVector other) => CompareTo(other) == 0;

    public override int GetHashCode()
    {
        unchecked { return (int)Checksum; }
    }

    public int CompareTo(ByteVector? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        int diff = Count - other.Count;

        for (int i = 0; diff == 0 && i < Count; i++)
            diff = this[i] - other[i];

        return diff;
    }

    public IEnumerator<byte> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

    public void Clear()
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects");

        _data.Clear();
    }

    public void Add(byte item)
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects");

        _data.Add(item);
    }

    public bool Remove(byte item)
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects");

        return _data.Remove(item);
    }

    public void CopyTo(byte[] array, int arrayIndex) => _data.CopyTo(array, arrayIndex);

    public bool Contains(byte item) => _data.Contains(item);

    public int Count => _data.Count;

    public void RemoveAt(int index)
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects");

        _data.RemoveAt(index);
    }

    public void Insert(int index, byte item)
    {
        if (IsReadOnly)
            throw new NotSupportedException("Cannot edit readonly objects");

        _data.Insert(index, item);
    }

    public int IndexOf(byte item) => _data.IndexOf(item);

    public virtual bool IsReadOnly => false;

    public virtual bool IsFixedSize => false;

    public byte this[int index]
    {
        get => _data[index];
        set
        {
            if (IsReadOnly)
                throw new NotSupportedException("Cannot edit readonly objects");

            _data[index] = value;
        }
    }

}
