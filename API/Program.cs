using API.service;

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

app.MapGet("/ctor", (services _service) => "I'm ready milord");

// app.MapGet("/run", (services _service) => _service.TestImage(null));

app.MapGet("/retrain", (services _service) => _service.ReTrain());

app.MapPost("/runimage", async (services _service, byte[] image) => await _service.TestImage(image));

app.Run();