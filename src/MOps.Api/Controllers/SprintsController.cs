using Microsoft.AspNetCore.Mvc;
using MOps.Core.Dtos;
using MOps.Core.Services;

namespace MOps.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:int}/sprints")]
public sealed class SprintsController(ISprintService sprints) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SprintDto>>> List(int projectId, CancellationToken ct)
        => await sprints.ListAsync(projectId, ct) is { } list ? Ok(list) : NotFound();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SprintDto>> Get(int projectId, int id, CancellationToken ct)
        => await sprints.GetAsync(projectId, id, ct) is { } sprint ? Ok(sprint) : NotFound();

    [HttpPost]
    public async Task<ActionResult<SprintDto>> Create(int projectId, SprintInput input, CancellationToken ct)
        => await sprints.CreateAsync(projectId, input, ct) is { } sprint
            ? CreatedAtAction(nameof(Get), new { projectId, id = sprint.Id }, sprint)
            : NotFound();

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SprintDto>> Update(int projectId, int id, SprintInput input, CancellationToken ct)
        => await sprints.UpdateAsync(projectId, id, input, ct) is { } sprint ? Ok(sprint) : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int projectId, int id, CancellationToken ct)
        => await sprints.DeleteAsync(projectId, id, ct) ? NoContent() : NotFound();

    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<SprintDto>> Activate(int projectId, int id, CancellationToken ct)
        => await sprints.ActivateAsync(projectId, id, ct) is { } sprint ? Ok(sprint) : NotFound();
}
