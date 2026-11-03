namespace MOps.Core.Repositories;

/// <summary>
/// Persiste as alterações pendentes feitas pelos repositórios (Add/Update/Remove).
/// Os repositórios só rastreiam mudanças; nada é gravado até <see cref="SaveChangesAsync"/>.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
