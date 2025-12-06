using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace CarRentalExamen.Blazor.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStorage _tokenStorage;
    private static readonly AuthenticationState AnonymousState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(TokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return AnonymousState;
        }

        var claims = ParseClaimsFromJwt(token);

        if (claims == null || !claims.Any())
        {
            return AnonymousState;
        }

        // Check if token is expired
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim != null && long.TryParse(expClaim.Value, out var expSeconds))
        {
            var expDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
            if (expDate < DateTime.UtcNow)
            {
                await _tokenStorage.ClearAsync();
                return AnonymousState;
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim>? ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            return token.Claims;
        }
        catch
        {
            return null;
        }
    }
}
