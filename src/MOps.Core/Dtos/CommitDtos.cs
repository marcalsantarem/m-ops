namespace MOps.Core.Dtos;

/// <summary>Payload enviado pelo hook post-commit.</summary>
public sealed record CommitHookInput(
    string? CommitHash,
    string? Message,
    string? Author,
    DateTimeOffset? CommittedAt);

/// <summary>
/// Resultado do vínculo de um commit. <see cref="Linked"/>: novos registros gravados;
/// <see cref="AlreadyLinked"/>: já existiam (reenvio idempotente); <see cref="NotFound"/>: <c>#id</c> sem work item.
/// </summary>
public sealed record CommitHookResult(
    string CommitHash,
    IReadOnlyList<int> Linked,
    IReadOnlyList<int> AlreadyLinked,
    IReadOnlyList<int> NotFound)
{
    /// <summary>Nenhuma referência <c>#id</c> encontrada na mensagem.</summary>
    public bool NoReferences => Linked.Count == 0 && AlreadyLinked.Count == 0 && NotFound.Count == 0;

    /// <summary>O commit está vinculado a pelo menos um work item (novo ou já existente).</summary>
    public bool HasAnyLink => Linked.Count > 0 || AlreadyLinked.Count > 0;
}
