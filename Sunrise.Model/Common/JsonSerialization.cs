using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Sunrise.Model.Common;

internal static class JsonSerialization
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public static void Serialize<TValue>(Stream stream, TValue value)
    {
        JsonSerializer.Serialize(stream, value, _options);
        stream.Flush();
    }

    public static byte[] Serialize<TValue>(TValue value)
        => JsonSerializer.SerializeToUtf8Bytes(value, _options);

    public static TValue? Deserialize<TValue>(Stream stream)
        => JsonSerializer.Deserialize<TValue>(stream, _options);

    public static TValue? Deserialize<TValue>(byte[] bytes)
        => JsonSerializer.Deserialize<TValue>(bytes, _options);
}
