using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NSE.Identidade.API.Data;
using NSE.Identidade.API.Extensions;
using System.Text;

namespace NSE.Identidade.API.Configuration;

public static class IdentityConfig
{
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        #region DbContex e Identity
        // Adding the DbContex
        services.AddDbContext<AppDbContext>();
        // Adding the Identity
        services.AddDefaultIdentity<IdentityUser>()
                        .AddRoles<IdentityRole>()
                        .AddErrorDescriber<IdentityMensagensPortugues>()
                        .AddEntityFrameworkStores<AppDbContext>()
                        .AddDefaultTokenProviders();
        #endregion

        #region Setando web token
        // AppSettings middleware service
        var appSettingsSection = configuration.GetSection("AppSettings"); // Buscando a sessão "AppSettings" no arquivo "appsettings.json" (por hora, o de dev)
        services.Configure<AppSettings>(appSettingsSection); // Inicializando uma instância de AppSettings com os valores pegos em "appSettingsSection"

        var appSettings = appSettingsSection.Get<AppSettings>(); // Pegando os dados da instância de AppSettings
        var key = Encoding.ASCII.GetBytes(appSettings.Secret); // Definindo o valor para a chave de criptografia

        // Setando Authentication
        services.AddAuthentication(options =>
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

        return services;
    }

    public static IApplicationBuilder UseIdentityConfiguration(this IApplicationBuilder app)
    {
        #region Autorização e Autenticação
        //Needs to be in this order
        app.UseAuthorization();
        app.UseAuthentication();
        #endregion

        return app;
    }
}
