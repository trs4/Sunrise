namespace Sunrise.Model.Common;

internal static class DataHelper
{
    public static bool Equals(string string1, string string2)
        => (string1 ?? string.Empty) == (string2 ?? string.Empty);

    public static bool Equals(DateTime dateTime1, DateTime dateTime2)
        => dateTime1.Year == dateTime2.Year && dateTime1.Month == dateTime2.Month && dateTime1.Day == dateTime2.Day
        && dateTime1.Hour == dateTime2.Hour && dateTime1.Minute == dateTime2.Minute && dateTime1.Second == dateTime2.Second;

    public static bool Equals(DateTime? dateTime1, DateTime? dateTime2)
        => (!dateTime1.HasValue && !dateTime2.HasValue) || (dateTime1.HasValue && dateTime2.HasValue && Equals(dateTime1.Value, dateTime2.Value));

    public static unsafe bool EqualBytesLongUnrolled(byte[] data1, byte[] data2)
    {
        if (data1 == data2)
            return true;

        if (data1 is null || data2 is null || data1.Length != data2.Length)
            return false;

        if (data1.Length == 0 && data2.Length == 0)
            return true;

        fixed (byte* bytes1 = data1, bytes2 = data2)
        {
            int len = data1.Length;
            int rem = len % (sizeof(long) * 16);
            long* b1 = (long*)bytes1;
            long* b2 = (long*)bytes2;
            long* e1 = (long*)(bytes1 + len - rem);

            while (b1 < e1)
            {
                if (*(b1) != *(b2) || *(b1 + 1) != *(b2 + 1) ||
                    *(b1 + 2) != *(b2 + 2) || *(b1 + 3) != *(b2 + 3) ||
                    *(b1 + 4) != *(b2 + 4) || *(b1 + 5) != *(b2 + 5) ||
                    *(b1 + 6) != *(b2 + 6) || *(b1 + 7) != *(b2 + 7) ||
                    *(b1 + 8) != *(b2 + 8) || *(b1 + 9) != *(b2 + 9) ||
                    *(b1 + 10) != *(b2 + 10) || *(b1 + 11) != *(b2 + 11) ||
                    *(b1 + 12) != *(b2 + 12) || *(b1 + 13) != *(b2 + 13) ||
                    *(b1 + 14) != *(b2 + 14) || *(b1 + 15) != *(b2 + 15))
                    return false;
                b1 += 16;
                b2 += 16;
            }

            for (int i = 0; i < rem; i++)
            {
                if (data1[len - 1 - i] != data2[len - 1 - i])
                    return false;
            }

            return true;
        }
    }

}
