namespace Portal.Client.Services.Instances;

/// <summary>
/// This object provides simple in-memory session storage for the duration of the user's session
/// Does not persist between sessions. So it is only used to store temporary state while the user is active.
/// Used for holding data that should not be persisted long-term. Search params, temporary selections, etc.
/// </summary>
public class SessionStorage
{
    /// <summary>
    /// Dictionary to hold session data
    /// </summary>
    private readonly Dictionary<string, object> _storage = [];
    /// <summary>
    /// Prevents concurrent access
    /// </summary>
    private readonly Lock _lock = new();

    /// <summary>
    /// Sets an item in the session storage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetItem<T>(string key, T value)
    {
        if (value is null)
            return;

        lock (_lock)
        {
            if (_storage.ContainsKey(key))
                _storage[key] = value;
            else
                _storage.Add(key, value);
        }
    }

    /// <summary>
    /// Gets an item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? GetItem<T>(string key)
    {
        lock (_lock)
        {
            if (_storage.TryGetValue(key, out object? value))
            {
                if (value is T typedValue)
                    return typedValue;
            }
            return default;
        }
    }

    /// <summary>
    /// Removes an item from the session storage
    /// </summary>
    /// <param name="key"></param>
    public void RemoveItem(string key)
    {
        lock (_lock)
            _ = _storage.Remove(key);
    }
}