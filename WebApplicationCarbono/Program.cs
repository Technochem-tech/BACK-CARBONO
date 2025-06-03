using Helpers;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;
using MercadoPago.Config; // <- SDK do Mercado Pago

var builder = WebApplication.CreateBuilder(args);

// Lê o accessToken da configuração
var mercadoPagoSettings = builder.Configuration.GetSection("MercadoPago").Get<MercadoPagoSettings>();
MercadoPagoConfig.AccessToken = mercadoPagoSettings.AccessToken;


// JWT e Swagger
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISaldo, SaldoServiço>();
builder.Services.AddScoped<ICreditos, CreditosServiço>();
builder.Services.AddScoped<IProjetos, ProjetosServiço>();
builder.Services.AddScoped<ITransaçao, TransacaoServiço>();
builder.Services.AddScoped<IUsuario, UsuarioServiço>();
builder.Services.AddScoped<IAutenticacao, AutenticacaoServiço>();
builder.Services.AddScoped<IPagamento, PagamentoServiço>();
builder.Services.AddScoped<IRedefinicaoSenha, RedefinicaoSenhaServiço>();
builder.Services.AddScoped<ITransferencia, TransferenciaServiço>();

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
