using CarRentalExamen.Blazor;
using CarRentalExamen.Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = new Uri("http://localhost:5263/");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiBase });
builder.Services.AddScoped<TokenStorage>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ApiClient>(sp =>
{
    var client = new ApiClient(
        sp.GetRequiredService<HttpClient>(),
        sp.GetRequiredService<TokenStorage>());

    client.SetAuthStateProvider(sp.GetRequiredService<CustomAuthStateProvider>());
    return client;
});

await builder.Build().RunAsync();
