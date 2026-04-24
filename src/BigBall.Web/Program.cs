using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.DependencyInjection;
using BigBall.Web;
using BigBall.Web.Platform;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = new Uri(builder.Configuration["ApiBase"]
                      ?? throw new InvalidOperationException("ApiBase missing in appsettings."));

builder.Services.AddBigBallClientCore(apiBase);
builder.Services.AddScoped<ITokenStore, LocalStorageTokenStore>();
builder.Services.AddScoped<IAppNavigator, BlazorAppNavigator>();

await builder.Build().RunAsync();
