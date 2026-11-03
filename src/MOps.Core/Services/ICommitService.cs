using MOps.Core.Dtos;

namespace MOps.Core.Services;

public interface ICommitService
{
    /// <summary>
    /// Extrai os <c>#id</c> da mensagem e grava um CommitRecord por work item existente.
    /// Reenvios do mesmo commit (WorkItemId + CommitHash) são ignorados sem erro.
    /// </summary>
    /// <exception cref="ValidationFailedException">Entrada inválida.</exception>
    Task<CommitHookResult> RegisterAsync(CommitHookInput input, CancellationToken cancellationToken = default);
}
