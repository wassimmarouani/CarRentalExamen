using Microsoft.JSInterop;

namespace CarRentalExamen.Blazor.Services;

public class TokenStorage
{
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "carRentalToken";
    private const string SessionStorageKey = "carRentalTokenSession";

    public TokenStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetTokenAsync(string token, bool rememberMe = true)
    {
        if (rememberMe)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, token);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", SessionStorageKey);
        }
        else
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", SessionStorageKey, token);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        var sessionToken = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", SessionStorageKey);
        if (!string.IsNullOrWhiteSpace(sessionToken))
        {
            return sessionToken;
        }

        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
    }

    public async Task ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", SessionStorageKey);
    }
}
