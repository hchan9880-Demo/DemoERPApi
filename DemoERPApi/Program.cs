var builder = WebApplication.CreateBuilder(args);
// services
// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/", () => "Demo ERP API is running!");
app.UseHttpsRedirection();

app.MapControllers();

// middleware
app.Run();

public partial class Program { }