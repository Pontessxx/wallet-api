namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IResetCodeValidator, ResetCodeValidator>();
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<ContaCarteiraService>();

        return services;
    }
}