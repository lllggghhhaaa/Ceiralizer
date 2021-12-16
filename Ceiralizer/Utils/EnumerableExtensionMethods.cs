namespace Ceiralizer.Utils;

public static class EnumerableExtensionMethods
{
    public static ArraySegment<T> GetSegment<T>(this IEnumerable<T> arr, int offset, int? count = null)
    {
        IEnumerable<T> enumerable = arr as T[] ?? arr.ToArray();
        
        if (count == null) { count = enumerable.Count() - offset; }
        return new ArraySegment<T>(enumerable.ToArray(), offset, count.Value);
    }
}