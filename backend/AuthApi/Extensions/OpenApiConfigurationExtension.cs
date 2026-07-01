namespace AuthApi.Extensions;

public static class OpenApiConfigurationExtension
{
    public static IServiceCollection AddOpenApiConfig(this IServiceCollection services)
    {
        var version = Environment.GetEnvironmentVariable("DOCKER_IMAGE_VERSION") ?? "dev";

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Auth API",
                Version = version,
                Description = "API da minha aplicação"
            });
            c.UseInlineDefinitionsForEnums();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT"
            });

            // c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            // {
            //     {
            //         new OpenApiSecuritySchemeReference("Bearer", document),
            //         new List<string>()
            //     }
            // });
            c.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerUiConfig(
        this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend API V1");
        });

        return app;
    }
}