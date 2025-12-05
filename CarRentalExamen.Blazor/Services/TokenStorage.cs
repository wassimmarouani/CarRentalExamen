using Microsoft.JSInterop;

namespace CarRentalExamen.Blazor.Services;

public class TokenStorage
{
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "carRentalToken";

    public TokenStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public Task SetTokenAsync(string token) => _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, token).AsTask();

    public async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
    }

    public Task ClearAsync() => _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey).AsTask();
}
