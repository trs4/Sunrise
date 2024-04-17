namespace Sunrise.Model.TagLib.Id3v2;

public static class SynchData
{
    public static uint ToUInt(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);
        uint sum = 0;
        int last = data.Count > 4 ? 3 : data.Count - 1;

        for (int i = 0; i <= last; i++)
            sum |= (uint)(data[i] & 0x7f) << ((last - i) * 7);

        return sum;
    }

    public static ByteVector FromUInt(uint value)
    {
        if ((value >> 28) != 0)
            throw new ArgumentOutOfRangeException(nameof(value), "value must be less than 268435456");

        ByteVector v = new ByteVector(4, 0);

        for (int i = 0; i < 4; i++)
            v[i] = (byte)(value >> ((3 - i) * 7) & 0x7f);

        return v;
    }

    public static void UnsynchByteVector(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);

        for (int i = data.Count - 2; i >= 0; i--)
        {
            if (data[i] == 0xFF && (data[i + 1] == 0 || (data[i + 1] & 0xE0) != 0))
                data.Insert(i + 1, 0);
        }
    }

    public static void ResynchByteVector(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);
        int i = 0, j = 0;

        while (i < data.Count - 1)
        {
            if (i != j)
                data[j] = data[i];

            i += data[i] == 0xFF && data[i + 1] == 0 ? 2 : 1;
            j++;
        }

        if (i < data.Count)
            data[j++] = data[i++];

        data.Resize(j);
    }

}
