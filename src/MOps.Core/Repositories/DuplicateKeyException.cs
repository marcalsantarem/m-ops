namespace MOps.Core.Repositories;

/// <summary>
/// Lançada por <see cref="IUnitOfWork.SaveChangesAsync"/> quando uma gravação viola um índice único.
/// As inserções que falharam já foram descartadas do contexto, então ele segue utilizável.
/// </summary>
public sealed class DuplicateKeyException(string message, Exception innerException)
    : Exception(message, innerException);
