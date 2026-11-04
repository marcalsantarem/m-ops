using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MOps.Mcp;

// Servidor MCP stdio (D-02): as tools chamam a API REST com token, sem acesso ao banco.
// Config: MOps:ApiUrl / MOps:ApiToken (env MOps__ApiUrl / MOps__ApiToken ou user-secrets).
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

// stdout é o canal MCP: todo log vai para stderr.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddOptions<MOpsApiOptions>().Bind(builder.Configuration.GetSection(MOpsApiOptions.Section));
builder.Services.AddHttpClient<MOpsApiClient>();

builder.Services
    .AddMcpServer(o => o.ServerInfo = new() { Name = "m-ops", Version = "1.0.0" })
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
