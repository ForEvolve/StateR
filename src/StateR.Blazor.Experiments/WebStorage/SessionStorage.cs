using Microsoft.JSInterop;

namespace StateR.Blazor.WebStorage;

public sealed class SessionStorage : Storage
{
    public SessionStorage(IJSInProcessRuntime jsInProcessRuntime, IJSRuntime jsRuntime)
        : base(StorageType.Session, jsInProcessRuntime, jsRuntime) { }
}
