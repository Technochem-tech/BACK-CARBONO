using Helpers;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Servi�os;
using MercadoPago.Config; // <- SDK do Mercado Pago

var builder = WebApplication.CreateBuilder(args);

// L� o accessToken da configura��o
var mercadoPagoSettings = builder.Configuration.GetSection("MercadoPago").Get<MercadoPagoSettings>();
MercadoPagoConfig.AccessToken = mercadoPagoSettings.AccessToken;


// JWT e Swagger
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Servi�os
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISaldo, SaldoServi�o>();
builder.Services.AddScoped<ICreditos, CreditosServi�o>();
builder.Services.AddScoped<IProjetos, ProjetosServi�o>();
builder.Services.AddScoped<ITransa�ao, TransacaoServi�o>();
builder.Services.AddScoped<IUsuario, UsuarioServi�o>();
builder.Services.AddScoped<IAutenticacao, AutenticacaoServi�o>();
builder.Services.AddScoped<IPagamento, PagamentoServi�o>();
builder.Services.AddScoped<IRedefinicaoSenha, RedefinicaoSenhaServi�o>();
builder.Services.AddScoped<ITransferencia, TransferenciaServi�o>();

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
