using Helpers;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;
using MercadoPago.Config; // <- SDK do Mercado Pago
using WebApplicationCarbono.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Lê o accessToken da configuração
var mercadoPagoSettings = builder.Configuration.GetSection("MercadoPago").Get<MercadoPagoSettings>() ?? throw new ArgumentNullException("MercadoPago", "As configurações do Mercado Pago não podem ser nulas.");
MercadoPagoConfig.AccessToken = mercadoPagoSettings.AccessToken;

// Registra o CORS (antes do Build)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("https://incandescent-sherbet-54d32d.netlify.app")
            .AllowAnyHeader()
            .AllowAnyMethod();
        //policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

    });
});

// JWT e Swagger
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISaldo, SaldoServiço>();
builder.Services.AddScoped<IProjetos, ProjetosServiço>();
builder.Services.AddScoped<IHistoricoTransacao, HistoricoTransacaoServiço>();
builder.Services.AddScoped<IUsuario, UsuarioServiço>();
builder.Services.AddScoped<IAutenticacao, AutenticacaoServiço>();
builder.Services.AddScoped<IPagamento, PagamentoServico>();
builder.Services.AddScoped<IRedefinicaoSenha, RedefinicaoSenhaServiço>();
builder.Services.AddScoped<ITransferirCredito, TransferirCreditoServiço>();
builder.Services.AddScoped<ICompraCreditos, CompraCreditosServico>();
builder.Services.AddScoped<IVendaCredito, VendaCreditoServico>();
builder.Services.AddScoped<PagamentoServico>();
builder.Services.AddHostedService<VerificadorDePagamentosService>();
builder.Services.AddScoped<IVerificacaoEmail, VerificacaoEmailServico>();
builder.Services.AddScoped<IBinanceServico, BinanceServico>();
// Registra BinanceServico e CompraBtcServico
builder.Services.AddSingleton<BinanceServico>();      // ou AddScoped, dependendo da sua necessidade
builder.Services.AddSingleton<CompraBtcServico>();    // necessário para o BackgroundService

// Registra o BackgroundService
builder.Services.AddHostedService<VerificadorDeComprasBtcService>();

builder.Services.AddSingleton<GmailServico>();





var app = builder.Build();



    app.UseSwagger();
    app.UseSwaggerUI();


app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");  // <-- middleware do CORS ativado aqui!

app.UseAuthorization();

app.MapControllers();

app.Run();
