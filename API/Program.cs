using API.service;
using API.trainer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<services>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.MapGet("/", () => "HELL ....");

app.MapGet("/ctor", (services _service) => "I'm ready milord");

// app.MapGet("/run", (services _service) => _service.TestImage(null));

app.MapGet("/retrain", (services _service) => _service.ReTrain());

app.MapPost("/runimage", (services _service, byte[] image) => _service.TestImage(image));

app.Run();