using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.DependencyInjection;
using BigBall.Web;
using BigBall.Web.Platform;
using BigBall.Web.Shared.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = new Uri(builder.Configuration["ApiBase"]
                      ?? throw new InvalidOperationException("ApiBase missing in appsettings."));

builder.Services.AddBigBallClientCore(apiBase);
builder.Services.AddScoped<ITokenStore, LocalStorageTokenStore>();
builder.Services.AddScoped<IUserProfileStore, LocalUserProfileStore>();
builder.Services.AddScoped<IUserTimeZoneProvider, UserTimeZoneProvider>();
builder.Services.AddScoped<IAppNavigator, BlazorAppNavigator>();
builder.Services.AddScoped<IAuthSession, WebAuthSession>();
builder.Services.AddScoped<ICreatePoolDialogService, CreatePoolDialogService>();
builder.Services.AddScoped<IJoinPoolDialogService, JoinPoolDialogService>();
builder.Services.AddScoped<IProfileEditDialogService, ProfileEditDialogService>();
builder.Services.AddScoped<TeamBadgeCache>();

await builder.Build().RunAsync();
