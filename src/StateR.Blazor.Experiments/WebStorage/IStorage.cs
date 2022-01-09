namespace StateR.Blazor.WebStorage;

public interface IStorage
{
    /// <summary>
    /// Gets the number of data items stored in a given <see cref="IStorage"/> object.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Returns the name of the nth key in a given <see cref="IStorage"/> object.
    /// The order of keys is user-agent defined, so you should not rely on it.
    /// </summary>
    /// <param name="index">
    /// The number of the key you want to get the name of.
    /// This is a zero-based index.
    /// </param>
    /// <returns>
    /// The name of the key. If the index does not exist, null is returned.
    /// </returns>
    string? Key(int index);

    /// <summary>
    /// Returns the specified key's value, or null if the key does not exist, in the
    /// given <see cref="IStorage"/> object.
    /// </summary>
    /// <param name="keyName">The name of the key you want to retrieve the value of.</param>
    /// <returns>The value of the key. If the key does not exist, null is returned.</returns>
    string? GetItem(string keyName);

    /// <summary>
    /// Adds the specified key to the given <see cref="IStorage"/> object, or
    /// updates that key's value if it already exists.
    /// </summary>
    /// <param name="keyName">The name of the key you want to create/update.</param>
    /// <param name="keyValue">The value you want to give the key you are creating/updating.</param>
    /// <remarks>
    /// setItem() may throw an exception if the storage is full. Particularly, in
    /// Mobile Safari (since iOS 5) it always throws when the user enters private
    /// mode. (Safari sets the quota to 0 bytes in private mode, unlike other browsers,
    /// which allow storage in private mode using separate data containers.) Hence
    /// developers should make sure to always catch possible exceptions from setItem().
    /// </remarks>
    void SetItem(string keyName, string keyValue);

    /// <summary>
    /// Removes the specified key from the given <see cref="IStorage"/> object if it exists.
    /// </summary>
    /// <param name="keyName">The name of the key you want to remove.</param>
    void RemoveItem(string keyName);

    /// <summary>
    /// Clears all keys stored in a given <see cref="IStorage"/> object.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the number of data items stored in a given <see cref="IStorage"/> object.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The number of data items stored in the <see cref="IStorage"/> object.</returns>
    ValueTask<int> GetLengthAsync(CancellationToken? cancellationToken = default);

    /// <summary>
    /// Returns the name of the nth key in a given <see cref="IStorage"/> object.
    /// The order of keys is user-agent defined, so you should not rely on it.
    /// </summary>
    /// <param name="index">
    /// The number of the key you want to get the name of.
    /// This is a zero-based index.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// The name of the key. If the index does not exist, null is returned.
    /// </returns>
    ValueTask<string?> KeyAsync(int index, CancellationToken? cancellationToken = default);

    /// <summary>
    /// Returns the specified key's value, or null if the key does not exist, in the
    /// given <see cref="IStorage"/> object.
    /// </summary>
    /// <param name="keyName">The name of the key you want to retrieve the value of.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The value of the key. If the key does not exist, null is returned.</returns>
    ValueTask<string?> GetItemAsync(string keyName, CancellationToken? cancellationToken = default);

    /// <summary>
    /// Adds the specified key to the given <see cref="IStorage"/> object, or
    /// updates that key's value if it already exists.
    /// </summary>
    /// <param name="keyName">The name of the key you want to create/update.</param>
    /// <param name="keyValue">The value you want to give the key you are creating/updating.</param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// setItem() may throw an exception if the storage is full. Particularly, in
    /// Mobile Safari (since iOS 5) it always throws when the user enters private
    /// mode. (Safari sets the quota to 0 bytes in private mode, unlike other browsers,
    /// which allow storage in private mode using separate data containers.) Hence
    /// developers should make sure to always catch possible exceptions from setItem().
    /// </remarks>
    ValueTask SetItemAsync(string keyName, string keyValue, CancellationToken? cancellationToken = default);

    /// <summary>
    /// Removes the specified key from the given <see cref="IStorage"/> object if it exists.
    /// </summary>
    /// <param name="keyName">The name of the key you want to remove.</param>
    /// <param name="cancellationToken"></param>
    ValueTask RemoveItemAsync(string keyName, CancellationToken? cancellationToken = default);

    /// <summary>
    /// Clears all keys stored in a given <see cref="IStorage"/> object.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask ClearAsync(CancellationToken? cancellationToken = default);
}
