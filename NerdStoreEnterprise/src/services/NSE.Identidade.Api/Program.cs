using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NSE.Identidade.API.Data;
using NSE.Identidade.API.Extensions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

#region Swagger doc
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NerdStore Enterprise Identity API",
        Description = "Esta API faz parte do curso ASP.NET Core Enterprise Applications",
        Contact = new OpenApiContact { Name = "Victor Almeida", Email = "victoralmdev@gmail.com" },
        License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") },
    });
});
#endregion

#region DbContex e Identity
// Adding the DbContex
builder.Services.AddDbContext<AppDbContext>();
// Adding the Identity
builder.Services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddErrorDescriber<IdentityMensagensPortugues>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
#endregion

#region Setando web token
// AppSettings middleware service
var appSettingsSection = builder.Configuration.GetSection("AppSettings"); // Buscando a sessão "AppSettings" no arquivo "appsettings.json" (por hora, o de dev)
builder.Services.Configure<AppSettings>(appSettingsSection); // Inicializando uma instância de AppSettings com os valores pegos em "appSettingsSection"

var appSettings = appSettingsSection.Get<AppSettings>(); // Pegando os dados da instância de AppSettings
var key = Encoding.ASCII.GetBytes(appSettings.Secret); // Definindo o valor para a chave de criptografia

// Setando Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(bearerOptions => // Adicionando options para este tipo específico de token
{
    bearerOptions.RequireHttpsMetadata = true; // requerendo acesso por https
    bearerOptions.SaveToken = true; // Salvando o token na instancia
    bearerOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, // validando o emissor com base na assinatura
        IssuerSigningKey = new SymmetricSecurityKey(key), // definindo a chave de assinatura
        ValidateIssuer = true, // Garantindo a validação do Issuer  (Não aceita tokens emitidos por outros emissores)
        ValidateAudience = true, // Garantindo a validação da Audience (Quais serão os domínios em que o token é valido)
        ValidAudience = appSettings.ValidoEm, // Definindo quais serão os domínios de Audience
        ValidIssuer = appSettings.Emissor, // Definindo quais serão os domínios de Issuer
    };
});
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseHttpsRedirection();

#region Autorização e Autenticação
//Needs to be in this order
app.UseAuthorization();
app.UseAuthentication();
#endregion

app.MapControllers();

app.Run();
