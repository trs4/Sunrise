using System.Buffers;

namespace Sunrise.Model.Common;

internal static class BuffersPool<T>
{
    private static bool IsPowerOfTwo(T[] array)
        => array.Length >= 512 && (array.Length & (array.Length - 1)) == 0;

    public static void ReturnSegment(ArraySegment<T> buffer, bool clearArray = false)
    {
        if (IsPowerOfTwo(buffer.Array))
            ArrayPool<T>.Shared.Return(buffer.Array, clearArray);
    }

}
