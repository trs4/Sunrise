namespace Sunrise.Model.Common;

public sealed class NaturalSortComparer : IComparer<string?>
{
    // Token values (not an enum as a performance micro-optimization)
    private const byte TokenRuLetters = 0;
    private const byte TokenLetters = 1;
    private const byte TokenNone = 2;
    private const byte TokenOther = 3;
    private const byte TokenDigits = 4;

    private NaturalSortComparer() { }

    public static NaturalSortComparer Instance { get; } = new();

    public int Compare(string? str1, string? str2)
    {
        if (str1 == str2)
            return 0;

        if (str1 is null)
            return -1;

        if (str2 is null)
            return 1;

        var strLength1 = str1.Length;
        var strLength2 = str2.Length;

        var startIndex1 = 0;
        var startIndex2 = 0;

        while (true)
        {
            // get next token from string 1
            var endIndex1 = startIndex1;
            var token1 = TokenNone;

            while (endIndex1 < strLength1)
            {
                var charToken = GetTokenFromChar(str1[endIndex1]);

                if (token1 == TokenNone)
                    token1 = charToken;
                else if (token1 != charToken)
                    break;

                endIndex1++;
            }

            // get next token from string 2
            var endIndex2 = startIndex2;
            var token2 = TokenNone;

            while (endIndex2 < strLength2)
            {
                var charToken = GetTokenFromChar(str2[endIndex2]);

                if (token2 == TokenNone)
                    token2 = charToken;
                else if (token2 != charToken)
                    break;

                endIndex2++;
            }

            // if the token kinds are different, compare just the token kind
            var tokenCompare = token1.CompareTo(token2);

            if (tokenCompare != 0)
                return tokenCompare;

            // now we know that both tokens are the same kind
            // didn't find any more tokens, return that they're equal
            if (token1 == TokenNone)
                return 0;

            var rangeLength1 = endIndex1 - startIndex1;
            var rangeLength2 = endIndex2 - startIndex2;

            if (token1 == TokenDigits)
            {
                // compare both tokens as numbers
                var maxLength = Math.Max(rangeLength1, rangeLength2);

                // both spans will get padded by zeroes on the left to be the same length
                const char paddingChar = '0';
                var paddingLength1 = maxLength - rangeLength1;
                var paddingLength2 = maxLength - rangeLength2;

                for (var i = 0; i < maxLength; i++)
                {
                    var digit1 = i < paddingLength1 ? paddingChar : str1[startIndex1 + i - paddingLength1];
                    var digit2 = i < paddingLength2 ? paddingChar : str2[startIndex2 + i - paddingLength2];

                    if (digit1 is >= '0' and <= '9' && digit2 is >= '0' and <= '9')
                    {
                        // both digits are ordinary 0 to 9
                        var digitCompare = digit1.CompareTo(digit2);

                        if (digitCompare != 0)
                            return digitCompare;
                    }
                    else
                    {
                        // one or both digits is unicode, compare parsed numeric values, and only if they are same, compare as char
                        var digitNumeric1 = char.GetNumericValue(digit1);
                        var digitNumeric2 = char.GetNumericValue(digit2);
                        var digitNumericCompare = digitNumeric1.CompareTo(digitNumeric2);

                        if (digitNumericCompare != 0)
                            return digitNumericCompare;

                        var digitCompare = digit1.CompareTo(digit2);

                        if (digitCompare != 0)
                            return digitCompare;
                    }
                }

                // if the numbers are equal, we compare how much we padded the strings
                var paddingCompare = paddingLength1.CompareTo(paddingLength2);

                if (paddingCompare != 0)
                    return paddingCompare;
            }
            else
            {
                // only compare non-numeric tokens up to the shorter of their lengths
                if (rangeLength1 < rangeLength2)
                {
                    rangeLength2 = rangeLength1;
                    endIndex2 = startIndex2 + rangeLength2;
                }
                else if (rangeLength2 < rangeLength1)
                {
                    rangeLength1 = rangeLength2;
                    endIndex1 = startIndex1 + rangeLength1;
                }

                // use string comparison
                var stringCompare = string.Compare(str1, startIndex1, str2, startIndex2, rangeLength1, StringComparison.CurrentCulture);

                if (stringCompare != 0)
                    return stringCompare;
            }

            startIndex1 = endIndex1;
            startIndex2 = endIndex2;
        }
    }

    private static byte GetTokenFromChar(char c) => c switch
    {
        >= 'а' and <= 'я' => TokenRuLetters,
        >= 'А' and <= 'Я' => TokenRuLetters,

        >= 'a' and <= 'z' => TokenLetters,
        >= 'a' when c < 128 => TokenOther,
        >= 'a' when char.IsLetter(c) => TokenLetters,
        >= 'a' when char.IsDigit(c) => TokenDigits,
        >= 'a' => TokenOther,
        >= 'A' and <= 'Z' => TokenLetters,
        >= 'A' => TokenOther,
        >= '0' and <= '9' => TokenDigits,
        _ => TokenOther,
    };

}
