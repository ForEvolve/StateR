namespace StateR.Blazor.WebStorage;

public sealed class WebStorage : IWebStorage
{
    public WebStorage(LocalStorage localStorage, SessionStorage sessionStorage)
    {
        LocalStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        SessionStorage = sessionStorage ?? throw new ArgumentNullException(nameof(sessionStorage));
    }

    public IStorage LocalStorage { get; }
    public IStorage SessionStorage { get; }
}
