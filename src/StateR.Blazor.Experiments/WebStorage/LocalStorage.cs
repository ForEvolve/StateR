using Microsoft.JSInterop;

namespace StateR.Blazor.WebStorage;

public sealed class LocalStorage : Storage
{
    public LocalStorage(IJSInProcessRuntime jsInProcessRuntime, IJSRuntime jsRuntime)
        : base(StorageType.Local, jsInProcessRuntime, jsRuntime) { }
}
