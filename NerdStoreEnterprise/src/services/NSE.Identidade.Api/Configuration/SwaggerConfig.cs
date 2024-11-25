﻿using Microsoft.OpenApi.Models;

namespace NSE.Identidade.API.Configuration;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        #region Swagger doc
        services.AddSwaggerGen(options =>
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

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        });

        return app;
    }
}
