namespace Portal.Server.Helpers;

/// <summary>
/// Extension methods for asynchronous enumerable operations
/// Provides utility methods for working with async enumerables
/// </summary>
public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Converts an async enumerable to a list asynchronously
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable</typeparam>
    /// <param name="source">The async enumerable to convert</param>
    /// <returns>A task that represents the asynchronous operation, containing the list of elements</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    public static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        return source == null ? throw new ArgumentNullException(nameof(source)) : ExecuteAsync();

        async Task<List<T>> ExecuteAsync()
        {
            List<T> list = new();

            await foreach (var element in source)
            {
                list.Add(element);
            }

            return list;
        }
    }
}
