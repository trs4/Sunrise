using System.Runtime.CompilerServices;
using System.Text;

namespace Sunrise.Model.Common;

public static class CacheStringBuilder
{
    [ThreadStatic]
    private static StringBuilder? _instance;
    private const int _defaultCapacity = 2048;

    [MethodImpl(Flags.HotPath)]
    public static StringBuilder Get()
    {
        var cachedInstance = _instance;

        if (cachedInstance is not null)
        {
            _instance = null;
            cachedInstance.Length = 0;
            return cachedInstance;
        }

        return new StringBuilder(_defaultCapacity);
    }

    [MethodImpl(Flags.HotPath)]
    public static string? ToString(StringBuilder builder)
    {
        if (builder is null)
            return null;

        string result = builder.ToString();
        _instance = builder.Capacity <= _defaultCapacity ? builder : null;
        return result;
    }

}
