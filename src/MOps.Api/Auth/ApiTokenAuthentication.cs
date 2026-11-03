using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MOps.Api.Auth;

/// <summary>Opções do esquema de token único (decisão D-03).</summary>
public sealed class ApiTokenAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>Token esperado (config <c>Auth:ApiToken</c>, env <c>Auth__ApiToken</c>). Vazio = tudo protegido responde 401.</summary>
    public string? ApiToken { get; set; }
}

/// <summary>
/// Aceita <c>Authorization: Bearer &lt;token&gt;</c> ou <c>X-Api-Key: &lt;token&gt;</c>, comparando em tempo constante.
/// Falha fechada: sem token configurado, nenhuma requisição é autenticada.
/// </summary>
public sealed class ApiTokenAuthenticationHandler(
    IOptionsMonitor<ApiTokenAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<ApiTokenAuthenticationOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiToken";
    public const string ApiKeyHeader = "X-Api-Key";
    public const string ConfigKey = "Auth:ApiToken";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var presented = ReadPresentedToken();
        if (presented is null) return Task.FromResult(AuthenticateResult.NoResult());

        var expected = Options.ApiToken;
        if (string.IsNullOrWhiteSpace(expected) || !TokensMatch(presented, expected))
            return Task.FromResult(AuthenticateResult.Fail("Token inválido."));

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "api-client")], Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = "Bearer";
        return Task.CompletedTask;
    }

    private string? ReadPresentedToken()
    {
        string? authorization = Request.Headers.Authorization;
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var bearer = authorization["Bearer ".Length..].Trim();
            if (bearer.Length > 0) return bearer;
        }

        string? apiKey = Request.Headers[ApiKeyHeader];
        return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim();
    }

    /// <summary>Compara os hashes SHA-256 em tempo constante (não vaza nem o tamanho do token).</summary>
    private static bool TokensMatch(string presented, string expected)
        => CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(presented)),
            SHA256.HashData(Encoding.UTF8.GetBytes(expected.Trim())));
}

public static class ApiTokenAuthenticationExtensions
{
    /// <summary>Registra o esquema de token como padrão e exige autenticação em todos os endpoints (fallback policy).</summary>
    public static IServiceCollection AddApiTokenAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(ApiTokenAuthenticationHandler.SchemeName)
            .AddScheme<ApiTokenAuthenticationOptions, ApiTokenAuthenticationHandler>(
                ApiTokenAuthenticationHandler.SchemeName,
                o => o.ApiToken = configuration[ApiTokenAuthenticationHandler.ConfigKey]);

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(ApiTokenAuthenticationHandler.SchemeName)
                .RequireAuthenticatedUser()
                .Build());
        return services;
    }
}
