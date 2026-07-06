var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApiConfig();
builder.Services.AddFrontendCorsConfig(builder.Configuration);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            message = "Ocorreu um erro interno no servidor.",
            traceId = context.TraceIdentifier,
            error = exceptionFeature?.Error.Message
        });
    });
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseSwaggerUiConfig();

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();