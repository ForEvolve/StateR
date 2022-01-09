using System.Diagnostics.CodeAnalysis;

namespace StateR.Blazor.WebStorage;

public static class StorageExtensions
{
    [NotNull]
    internal static WebStorageOptions? WebStorageOptions { get; set; }

    public static void SetItem<T>(this IStorage storage, string keyName, T keyValue)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(storage, nameof(storage));
        var value = WebStorageOptions.Serializer.Serialize(keyValue);
        storage.SetItem(keyName, value);
    }

    public static T? GetItem<T>(this IStorage storage, string keyName)
    {
        ArgumentNullException.ThrowIfNull(storage, nameof(storage));
        var rawValue = storage.GetItem(keyName);
        if( rawValue == null)
        {
            return default;
        }
        var value = WebStorageOptions.Serializer.Deserialize<T>(rawValue);
        return value;
    }

    public static async ValueTask SetItemAsync<T>(this IStorage storage, string keyName, T keyValue, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(storage, nameof(storage));
        var value = await WebStorageOptions.Serializer.SerializeAsync(keyValue, cancellationToken);
        await storage.SetItemAsync(keyName, value, cancellationToken);
    }

    public static async ValueTask<T?> GetItemAsync<T>(this IStorage storage, string keyName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(storage, nameof(storage));
        var rawValue = await storage.GetItemAsync(keyName, cancellationToken);
        if (rawValue == null)
        {
            return default;
        }
        var value = await WebStorageOptions.Serializer.DeserializeAsync<T>(rawValue, cancellationToken);
        return value;
    }
}