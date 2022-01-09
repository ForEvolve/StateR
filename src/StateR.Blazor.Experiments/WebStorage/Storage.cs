using Microsoft.JSInterop;

namespace StateR.Blazor.WebStorage;

public abstract class Storage : IStorage
{
    private readonly string _clearIdentifier;
    private readonly string _lengthIdentifier;
    private readonly string _getItemIdentifier;
    private readonly string _keyIdentifier;
    private readonly string _removeItemIdentifier;
    private readonly string _setItemIdentifier;

    private readonly IJSInProcessRuntime _jsInProcessRuntime;
    private readonly IJSRuntime _jsRuntime;

    public Storage(StorageType storageType, IJSInProcessRuntime jsInProcessRuntime, IJSRuntime jsRuntime)
    {
        _jsInProcessRuntime = jsInProcessRuntime ?? throw new ArgumentNullException(nameof(jsInProcessRuntime));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));

        var windowPropertyName = storageType == StorageType.Session
            ? "sessionStorage"
            : "localStorage";
        _lengthIdentifier = $"{windowPropertyName}.length";
        _clearIdentifier = $"{windowPropertyName}.clear";
        _getItemIdentifier = $"{windowPropertyName}.getItem";
        _keyIdentifier = $"{windowPropertyName}.key";
        _removeItemIdentifier = $"{windowPropertyName}.removeItem";
        _setItemIdentifier = $"{windowPropertyName}.setItem";
    }

    public int Length => _jsInProcessRuntime.Invoke<int>("eval", _lengthIdentifier);
    public ValueTask<int> GetLengthAsync(CancellationToken? cancellationToken = null)
        => _jsRuntime.InvokeAsync<int>("eval", cancellationToken ?? CancellationToken.None, _lengthIdentifier);

    public void Clear()
        => _jsInProcessRuntime.InvokeVoid(_clearIdentifier);
    public ValueTask ClearAsync(CancellationToken? cancellationToken = null)
        => _jsRuntime.InvokeVoidAsync(_clearIdentifier, cancellationToken);

    public string? GetItem(string keyName)
        => _jsInProcessRuntime.Invoke<string?>(_getItemIdentifier, keyName);
    public ValueTask<string?> GetItemAsync(string keyName, CancellationToken? cancellationToken = null)
        => _jsRuntime.InvokeAsync<string?>(_getItemIdentifier, cancellationToken ?? CancellationToken.None, keyName);

    public string? Key(int index)
        => _jsInProcessRuntime.Invoke<string?>(_keyIdentifier, index);
    public ValueTask<string?> KeyAsync(int index, CancellationToken? cancellationToken = null)
        => _jsRuntime.InvokeAsync<string?>(_keyIdentifier, cancellationToken ?? CancellationToken.None, index);

    public void RemoveItem(string keyName)
        => _jsInProcessRuntime.Invoke<string>(_removeItemIdentifier, keyName);
    public ValueTask RemoveItemAsync(string keyName, CancellationToken? cancellationToken = null)
        => _jsRuntime.InvokeVoidAsync(_removeItemIdentifier, cancellationToken ?? CancellationToken.None, keyName);

    public void SetItem(string keyName, string keyValue)
        => _jsInProcessRuntime.Invoke<string>(_setItemIdentifier, keyName, keyValue);
    public ValueTask SetItemAsync(string keyName, string keyValue, CancellationToken? cancellationToken = null)
        => _jsRuntime.InvokeVoidAsync(_setItemIdentifier, cancellationToken ?? CancellationToken.None, keyName, keyValue);
}
