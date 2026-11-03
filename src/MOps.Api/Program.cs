using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MOps.Api.Auth;
using MOps.Api.Filters;
using MOps.Infrastructure;
using MOps.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddControllers(options => options.Filters.Add<ValidationFailedExceptionFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// CORS: origens em Cors:AllowedOrigins (env: Cors__AllowedOrigins__0, __1...). Sem origens, nenhuma é liberada.
var allowedOrigins = (builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .Where(o => !string.IsNullOrWhiteSpace(o))
    .Select(o => o.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (allowedOrigins.Length > 0)
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
}));

// Token único (D-03): Auth:ApiToken (env Auth__ApiToken). Tudo exige token, exceto /health e o OpenAPI em Development.
builder.Services.AddApiTokenAuthentication(builder.Configuration);

var app = builder.Build();

if (string.IsNullOrWhiteSpace(app.Configuration[ApiTokenAuthenticationHandler.ConfigKey]))
    app.Logger.LogWarning("Auth:ApiToken não configurado: todos os endpoints protegidos responderão 401.");

// Aplica as migrations pendentes na inicialização (inclusive no deploy do Render).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MOpsDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseHttpsRedirection();

// CORS antes da autenticação: o preflight (OPTIONS) é respondido pelo middleware de CORS sem exigir token.
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapControllers();

app.Run();
