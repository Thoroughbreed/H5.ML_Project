using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// TODO Fix the goddamn URI :)
builder.Services.AddScoped(sp => 
    new HttpClient { BaseAddress = new Uri("http://62.66.208.26:8089") });

await builder.Build().RunAsync();