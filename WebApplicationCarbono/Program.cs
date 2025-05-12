using Helpers;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;

var builder = WebApplication.CreateBuilder(args);

// Configuração do JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configutações para aparecer o Campo  de add o token
builder.Services.AddSwaggerDocumentation();


// Adicionar serviços ao contêiner
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração de serviços
builder.Services.AddScoped<ISaldo, SaldoServiços>();
builder.Services.AddScoped<ICreditos, CreditosServiços>();
builder.Services.AddScoped<IProjetos, ProjetosServiços>();
builder.Services.AddScoped<ITransaçao, TransacaoServiços>();
builder.Services.AddScoped<IUsuario, UsuarioServiço>();
builder.Services.AddScoped<IAutenticacao, AutenticacaoServico>();

var app = builder.Build();

// Configurar o pipeline de requisição
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
