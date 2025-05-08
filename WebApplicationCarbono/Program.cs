using Microsoft.Extensions.Configuration;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurações de serviço (Registrar antes de chamar builder.Build())
builder.Services.AddScoped<ISaldo, SaldoServiços>();
builder.Services.AddScoped<ICreditos, CreditosServiços>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
