using Helpers;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;
using MercadoPago.Config; // <- SDK do Mercado Pago

var builder = WebApplication.CreateBuilder(args);

// ? Lê o accessToken da configuração
var mercadoPagoSettings = builder.Configuration.GetSection("MercadoPago").Get<MercadoPagoSettings>();
MercadoPagoConfig.AccessToken = mercadoPagoSettings.AccessToken;

// JWT e Swagger
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISaldo, SaldoServiços>();
builder.Services.AddScoped<ICreditos, CreditosServiços>();
builder.Services.AddScoped<IProjetos, ProjetosServiços>();
builder.Services.AddScoped<ITransaçao, TransacaoServiços>();
builder.Services.AddScoped<IUsuario, UsuarioServiço>();
builder.Services.AddScoped<IAutenticacao, AutenticacaoServico>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
