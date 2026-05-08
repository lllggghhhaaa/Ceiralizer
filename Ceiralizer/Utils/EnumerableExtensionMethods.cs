namespace Ceiralizer.Utils;

public static class EnumerableExtensionMethods
{
    /// <summary>
    /// Get an segment of enumerable
    /// </summary>
    /// <param name="arr">Original list</param>
    /// <param name="offset">Start index of the segment</param>
    /// <param name="count">Lenght of the segment</param>
    /// <typeparam name="T">IEnumerable generic type</typeparam>
    /// <returns></returns>
    public static ArraySegment<T> GetSegment<T>(this IEnumerable<T> arr, int offset, int? count = null)
    {
        IEnumerable<T> enumerable = arr as T[] ?? arr.ToArray();
        
        count ??= enumerable.Count() - offset;
        return new ArraySegment<T>(enumerable.ToArray(), offset, count.Value);
    }
}