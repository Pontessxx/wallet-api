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
            c.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "Auth API V2",
                Version = version,
                Description = "Autenticação com access token, refresh token e cookie HttpOnly"
            });
            c.UseInlineDefinitionsForEnums();

            c.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (!apiDesc.TryGetMethodInfo(out var methodInfo))
                    return false;

                var groupName = methodInfo.DeclaringType?
                    .GetCustomAttributes(true)
                    .OfType<ApiExplorerSettingsAttribute>()
                    .FirstOrDefault()?.GroupName;

                return groupName == docName;
            });

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

    public static IServiceCollection AddFrontendCorsConfig(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    policy.AllowAnyOrigin();
                }

                policy.AllowAnyHeader().AllowAnyMethod();
            });
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
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "Backend API V2");
        });

        return app;
    }
}