using Microsoft.AspNetCore.Mvc;
using MOps.Core.Dtos;
using MOps.Core.Services;

namespace MOps.Api.Controllers;

[ApiController]
public sealed class CommitsController(ICommitService commits) : ControllerBase
{
    /// <summary>
    /// Recebe o commit do hook post-commit. 201 se gravou algum vínculo novo, 200 se todos já existiam
    /// (reenvio), 422 se a mensagem não referencia nenhum work item existente.
    /// </summary>
    [HttpPost("api/commits/hook")]
    public async Task<ActionResult<CommitHookResult>> Hook(CommitHookInput input, CancellationToken ct)
    {
        var result = await commits.RegisterAsync(input, ct);
        if (result.Linked.Count > 0) return StatusCode(StatusCodes.Status201Created, result);
        if (result.HasAnyLink) return Ok(result);

        var detail = result.NoReferences
            ? "A mensagem não contém nenhuma referência #<id> a work item."
            : "Nenhum work item existente para: " + string.Join(", ", result.NotFound.Select(id => "#" + id)) + ".";
        return Problem(detail: detail, statusCode: StatusCodes.Status422UnprocessableEntity, title: "Commit não vinculado.");
    }
}
