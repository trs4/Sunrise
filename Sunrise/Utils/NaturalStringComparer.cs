using System;
using System.Collections.Generic;
using System.Numerics;

namespace Sunrise.Utils;

public sealed class NaturalStringComparer : IComparer<string?>, IComparer<ReadOnlyMemory<char>>
{
    private readonly StringComparison _comparison;

    public static NaturalStringComparer Instance { get; } = new();

    public NaturalStringComparer(StringComparison comparison = StringComparison.Ordinal)
        => _comparison = comparison;

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
            return 0;

        if (x is null)
            return -1;

        if (y is null)
            return 1;

        return Compare(x.AsSpan(), y.AsSpan(), _comparison);
    }

    public int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y) => Compare(x, y, _comparison);

    public int Compare(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) => Compare(x.Span, y.Span, _comparison);

    public static int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y, StringComparison stringComparison)
    {
        var length = Math.Min(x.Length, y.Length);

        for (var i = 0; i < length; i++)
        {
            var xCh = x[i];
            var yCh = y[i];

            if (char.IsDigit(xCh) && char.IsDigit(yCh))
            {
                var xOut = GetNumber(x.Slice(i), out var xNum);
                var yOut = GetNumber(y.Slice(i), out var yNum);

                UnifyNumberTypes(ref xNum, ref yNum);
                var compareResult = xNum.CompareTo(yNum);

                if (compareResult != 0)
                    return compareResult;

                i = -1;
                length = Math.Min(xOut.Length, yOut.Length);

                if (length == 0 && xOut.Length == yOut.Length && x.Length != y.Length)
                    return y.Length < x.Length ? -1 : 1; // "033" < "33" === true
                else
                {
                    x = xOut;
                    y = yOut;
                    continue;
                }
            }

            if (xCh != yCh)
                return x.Slice(i, 1).CompareTo(y.Slice(i, 1), stringComparison);
        }

        return x.Length.CompareTo(y.Length);
    }

    private static ReadOnlySpan<char> GetNumber(ReadOnlySpan<char> span, out IComparable number)
    {
        var i = 0;

        while (i < span.Length && char.IsDigit(span[i]))
            i++;

        var parseInput = span[..i];
        number = ulong.TryParse(parseInput, out var ulongResult) ? ulongResult : BigInteger.Parse(parseInput);
        return span.Slice(i);
    }

    private static void UnifyNumberTypes(ref IComparable x, ref IComparable y)
    {
        if (x is ulong xLong && y is BigInteger)
            x = new BigInteger(xLong);

        if (x is BigInteger && y is ulong yLong)
            y = new BigInteger(yLong);
    }

}
