namespace MOps.Core.Repositories;

/// <summary>
/// Persiste as alterações pendentes feitas pelos repositórios (Add/Update/Remove).
/// Os repositórios só rastreiam mudanças; nada é gravado até <see cref="SaveChangesAsync"/>.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa <paramref name="operation"/> (que pode chamar <see cref="SaveChangesAsync"/> várias vezes)
    /// numa única transação; confirma ao final ou desfaz tudo se houver exceção.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
