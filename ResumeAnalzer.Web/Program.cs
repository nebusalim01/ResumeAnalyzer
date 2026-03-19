using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ResumeAnalzer.Web;
using ResumeAnalzer.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Point to our API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7242/")
});

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ResumeService>();

await builder.Build().RunAsync();