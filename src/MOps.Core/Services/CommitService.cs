using System.Globalization;
using System.Text.RegularExpressions;
using MOps.Core.Dtos;
using MOps.Core.Entities;
using MOps.Core.Repositories;

namespace MOps.Core.Services;

public sealed partial class CommitService(
    IWorkItemRepository workItems,
    ICommitRecordRepository commits,
    IUnitOfWork unitOfWork) : ICommitService
{
    public const int CommitHashMaxLength = 64;
    public const int MessageMaxLength = 4000;
    public const int AuthorMaxLength = 200;

    // "#42" não colado a letra/dígito (ignora "abc#1", "&#39;" e "#12a").
    [GeneratedRegex(@"(?<![\w&])#(\d{1,9})(?!\w)")]
    private static partial Regex WorkItemReference();

    public async Task<CommitHookResult> RegisterAsync(CommitHookInput input, CancellationToken cancellationToken = default)
    {
        var data = Validate(input);
        var hash = data.CommitHash!;
        var committedAt = data.CommittedAt!.Value.ToUniversalTime();

        List<int> linked = [], alreadyLinked = [], notFound = [];
        foreach (var workItemId in ExtractWorkItemIds(data.Message!))
        {
            if (!await workItems.ExistsAsync(workItemId, cancellationToken)) { notFound.Add(workItemId); continue; }
            if (await commits.ExistsAsync(workItemId, hash, cancellationToken)) { alreadyLinked.Add(workItemId); continue; }

            commits.Add(new CommitRecord
            {
                WorkItemId = workItemId,
                CommitHash = hash,
                Message = data.Message!,
                Author = data.Author!,
                CommittedAt = committedAt
            });
            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                linked.Add(workItemId);
            }
            catch (DuplicateKeyException)
            {
                // Corrida com outro envio do mesmo commit: o índice único garante a idempotência.
                alreadyLinked.Add(workItemId);
            }
        }

        return new CommitHookResult(hash, linked, alreadyLinked, notFound);
    }

    public async Task<IReadOnlyList<CommitRecordDto>?> ListByWorkItemAsync(int workItemId, CancellationToken cancellationToken = default)
    {
        if (!await workItems.ExistsAsync(workItemId, cancellationToken)) return null;
        var list = await commits.ListByWorkItemAsync(workItemId, cancellationToken);
        return list
            .Select(c => new CommitRecordDto(c.Id, c.CommitHash, c.Message, c.Author, c.CommittedAt))
            .ToList();
    }

    /// <summary>IDs distintos referenciados como <c>#id</c>, na ordem em que aparecem.</summary>
    public static IReadOnlyList<int> ExtractWorkItemIds(string message)
        => WorkItemReference().Matches(message)
            .Select(m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
            .Where(id => id > 0)
            .Distinct()
            .ToList();

    /// <summary>Normaliza (trim) e valida; lança <see cref="ValidationFailedException"/>.</summary>
    private static CommitHookInput Validate(CommitHookInput? input)
    {
        var hash = string.IsNullOrWhiteSpace(input?.CommitHash) ? null : input.CommitHash.Trim();
        var message = string.IsNullOrWhiteSpace(input?.Message) ? null : input.Message.Trim();
        var author = string.IsNullOrWhiteSpace(input?.Author) ? null : input.Author.Trim();
        var errors = new Dictionary<string, string[]>();

        if (hash is null)
            errors[nameof(CommitHookInput.CommitHash)] = ["CommitHash é obrigatório."];
        else if (hash.Length > CommitHashMaxLength)
            errors[nameof(CommitHookInput.CommitHash)] = [$"CommitHash deve ter no máximo {CommitHashMaxLength} caracteres."];

        if (message is null)
            errors[nameof(CommitHookInput.Message)] = ["Message é obrigatório."];
        else if (message.Length > MessageMaxLength)
            errors[nameof(CommitHookInput.Message)] = [$"Message deve ter no máximo {MessageMaxLength} caracteres."];

        if (author is null)
            errors[nameof(CommitHookInput.Author)] = ["Author é obrigatório."];
        else if (author.Length > AuthorMaxLength)
            errors[nameof(CommitHookInput.Author)] = [$"Author deve ter no máximo {AuthorMaxLength} caracteres."];

        if (input?.CommittedAt is null)
            errors[nameof(CommitHookInput.CommittedAt)] = ["CommittedAt é obrigatório."];

        if (errors.Count > 0) throw new ValidationFailedException(errors);
        return input! with { CommitHash = hash, Message = message, Author = author };
    }
}
