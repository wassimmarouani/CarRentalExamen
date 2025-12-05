using CarRentalExamen.Blazor;
using CarRentalExamen.Blazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = new Uri("https://localhost:7152/");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiBase });
builder.Services.AddScoped<TokenStorage>();
builder.Services.AddScoped<ApiClient>();

await builder.Build().RunAsync();
