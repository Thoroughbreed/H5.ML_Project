using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// TODO Fix the goddamn URI :)
builder.Services.AddScoped(sp => 
    new HttpClient { BaseAddress = new Uri("https://localhost:7258") });

await builder.Build().RunAsync();