using Helpers;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Servi�os;
using MercadoPago.Config; // <- SDK do Mercado Pago

var builder = WebApplication.CreateBuilder(args);

// L� o accessToken da configura��o
var mercadoPagoSettings = builder.Configuration.GetSection("MercadoPago").Get<MercadoPagoSettings>() ?? throw new ArgumentNullException("MercadoPago", "As configura��es do Mercado Pago n�o podem ser nulas.");
MercadoPagoConfig.AccessToken = mercadoPagoSettings.AccessToken;

// Registra o CORS (antes do Build)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:8080") // porta do seu front-end
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT e Swagger
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Servi�os
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISaldo, SaldoServi�o>();
builder.Services.AddScoped<IProjetos, ProjetosServi�o>();
builder.Services.AddScoped<IHistoricoTransacao, HistoricoTransacaoServi�o>();
builder.Services.AddScoped<IUsuario, UsuarioServi�o>();
builder.Services.AddScoped<IAutenticacao, AutenticacaoServi�o>();
builder.Services.AddScoped<IPagamento, PagamentoServico>();
builder.Services.AddScoped<IRedefinicaoSenha, RedefinicaoSenhaServi�o>();
builder.Services.AddScoped<ITransferirCredito, TransferirCreditoServi�o>();
builder.Services.AddScoped<ICompraCreditos, CompraCreditosServico>();
builder.Services.AddScoped<IVendaCredito, VendaCreditoServico>();
builder.Services.AddScoped<PagamentoServico>();
builder.Services.AddHostedService<VerificadorDePagamentosService>();




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");  // <-- middleware do CORS ativado aqui!

app.UseAuthorization();

app.MapControllers();

app.Run();
