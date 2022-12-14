var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddCors();
// builder.Services.AddScoped(sp => 
//     new HttpClient { BaseAddress = new Uri("http://62.66.208.26:8089") });
builder.Services.AddScoped(sp => 
    new HttpClient { BaseAddress = new Uri("http://192.168.2.10:8080") });

// REMEMBER TO ADD THIS TO JSON
// ;https://192.168.2.10:7175

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(o =>
{
    o.AllowAnyHeader();
    o.AllowAnyMethod();
    o.AllowAnyOrigin();
});

app.UseAuthorization();

app.MapRazorPages();

app.Run();