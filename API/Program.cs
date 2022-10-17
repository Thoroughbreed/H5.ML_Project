using API.trainer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<Trainer>();

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

app.MapGet("/ctor", (Trainer _trainer) =>
{
    _trainer.TrainData();
    return "What the f....?";
});

app.MapGet("/run", (Trainer _trainer) =>
{
    _trainer.RunSingleImage();
    return "What the f....?";
});

app.Run();