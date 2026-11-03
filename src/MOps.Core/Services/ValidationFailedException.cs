namespace MOps.Core.Services;

/// <summary>
/// Lançada pelos serviços de aplicação quando a entrada é inválida.
/// <see cref="Errors"/> segue o formato de ValidationProblemDetails (campo → mensagens).
/// </summary>
public sealed class ValidationFailedException(IDictionary<string, string[]> errors)
    : Exception("Um ou mais campos são inválidos.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
