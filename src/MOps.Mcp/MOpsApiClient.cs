using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using MOps.Core.Dtos;
using MOps.Core.Enums;

namespace MOps.Mcp;

public sealed class MOpsApiOptions
{
    public const string Section = "MOps";

    /// <summary>URL base da API (ex.: https://m-ops.onrender.com).</summary>
    public string? ApiUrl { get; set; }

    /// <summary>Token enviado como <c>Authorization: Bearer</c> (D-03).</summary>
    public string? ApiToken { get; set; }
}

/// <summary>
/// Cliente tipado da API REST do M-Ops. Falhas HTTP viram <see cref="McpException"/> com mensagem
/// legível, que o SDK devolve ao agente como resultado de erro da tool.
/// </summary>
public sealed class MOpsApiClient
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;

    public MOpsApiClient(HttpClient http, IOptions<MOpsApiOptions> options)
    {
        _http = http;
        var o = options.Value;
        if (!string.IsNullOrWhiteSpace(o.ApiUrl) && Uri.TryCreate(o.ApiUrl.TrimEnd('/') + "/", UriKind.Absolute, out var baseUri))
            _http.BaseAddress = baseUri;
        if (!string.IsNullOrWhiteSpace(o.ApiToken))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", o.ApiToken);
    }

    public Task<List<ProjectDto>> GetProjectsAsync(CancellationToken ct) =>
        SendAsync<List<ProjectDto>>(HttpMethod.Get, "api/projects", null, ct);

    public Task<ProjectDto> GetProjectAsync(int id, CancellationToken ct) =>
        SendAsync<ProjectDto>(HttpMethod.Get, $"api/projects/{id}", null, ct);

    public Task<List<SprintDto>> GetSprintsAsync(int projectId, CancellationToken ct) =>
        SendAsync<List<SprintDto>>(HttpMethod.Get, $"api/projects/{projectId}/sprints", null, ct);

    public Task<List<WorkItemDto>> GetWorkItemsAsync(int projectId, int? sprintId, WorkItemStatus? status, string? keyword, CancellationToken ct)
    {
        var q = new List<string>();
        if (sprintId is not null) q.Add($"sprintId={sprintId}");
        if (status is not null) q.Add($"status={status}");
        if (!string.IsNullOrWhiteSpace(keyword)) q.Add($"keyword={Uri.EscapeDataString(keyword)}");
        var url = $"api/projects/{projectId}/work-items" + (q.Count > 0 ? "?" + string.Join('&', q) : "");
        return SendAsync<List<WorkItemDto>>(HttpMethod.Get, url, null, ct);
    }

    public Task<WorkItemDto> CreateWorkItemAsync(int projectId, WorkItemInput input, CancellationToken ct) =>
        SendAsync<WorkItemDto>(HttpMethod.Post, $"api/projects/{projectId}/work-items", input, ct);

    public Task<WorkItemDto> UpdateStatusAsync(int workItemId, WorkItemStatus status, CancellationToken ct) =>
        SendAsync<WorkItemDto>(HttpMethod.Patch, $"api/work-items/{workItemId}/status", new WorkItemStatusInput(status), ct);

    public Task<CommitHookResult> PostCommitAsync(CommitHookInput input, CancellationToken ct) =>
        SendAsync<CommitHookResult>(HttpMethod.Post, "api/commits/hook", input, ct);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        if (_http.BaseAddress is null)
            throw new McpException("MOps:ApiUrl não configurado. Defina a variável de ambiente MOps__ApiUrl com a URL da API do M-Ops.");

        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body, body.GetType(), options: Json);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new McpException($"Não foi possível contatar a API do M-Ops em {_http.BaseAddress}: {ex.Message}");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new McpException($"Tempo esgotado ao chamar a API do M-Ops ({method} /{path}).");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new McpException(await DescribeErrorAsync(response, method, path, ct));

            var result = await response.Content.ReadFromJsonAsync<T>(Json, ct);
            return result ?? throw new McpException($"Resposta vazia da API ({method} /{path}).");
        }
    }

    private static async Task<string> DescribeErrorAsync(HttpResponseMessage response, HttpMethod method, string path, CancellationToken ct)
    {
        var code = (int)response.StatusCode;
        var prefix = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                "Acesso negado pela API (token ausente ou inválido; confira MOps__ApiToken).",
            HttpStatusCode.NotFound => "Recurso não encontrado.",
            HttpStatusCode.BadRequest => "Requisição inválida.",
            HttpStatusCode.UnprocessableEntity => "Operação não processada.",
            _ => "Erro da API.",
        };

        var detail = "";
        try
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            if (!string.IsNullOrWhiteSpace(text))
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                var parts = new List<string>();
                if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String) parts.Add(t.GetString()!);
                if (root.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String) parts.Add(d.GetString()!);
                if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Object)
                    foreach (var field in errs.EnumerateObject())
                        foreach (var msg in field.Value.EnumerateArray())
                            parts.Add($"{field.Name}: {msg.GetString()}");
                detail = string.Join(" ", parts);
            }
        }
        catch (JsonException) { }

        return $"{prefix} HTTP {code} em {method} /{path}." + (detail.Length > 0 ? " " + detail : "");
    }
}
