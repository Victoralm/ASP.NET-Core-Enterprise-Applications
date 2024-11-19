using NSE.Identidade.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApiConfiguration();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerConfiguration();

#region Configurações relativas ao ambiente atual
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true) // Adicionando o Json com configurações gerais
                .AddJsonFile($"appsettings.{environment}.json", true, true) // Adicionando o Json com configurações específicas do ambiente em que se está no momento
                .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>();
#endregion

builder.Services.AddIdentityConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}

app.UseApiConfiguration();

app.MapControllers();

app.Run();
