using API.Interfaces;
using API.service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IServices, Services>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// For debugging purposes :)
app.UseCors(o =>
{
    o.AllowAnyHeader();
    o.AllowAnyMethod();
    o.AllowAnyOrigin();
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.MapGet("/", () => "HELL ....");

app.MapGet("/ctor", (IServices service) => "I'm ready milord"); // Starts the IService when the API starts - required :)

app.MapGet("/retrain", (IServices service) => service.ReTrain());

app.MapGet("/captcha", (IServices service) => service.Captcha());
app.MapPost("/captcha", (IServices service, Tuple<string, string> reply) => service.CaptchaReturn(reply));

app.MapPost("/delete", (IServices service, string path) => service.DeleteWrong(path));

app.MapPost("/runimage", async (IServices service, byte[] image) => await service.TestImage(image));
app.Run();