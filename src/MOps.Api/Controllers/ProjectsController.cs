using Microsoft.AspNetCore.Mvc;
using MOps.Core.Dtos;
using MOps.Core.Services;

namespace MOps.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController(IProjectService projects) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> List(CancellationToken ct)
        => Ok(await projects.ListAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> Get(int id, CancellationToken ct)
        => await projects.GetAsync(id, ct) is { } project ? Ok(project) : NotFound();

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(ProjectInput input, CancellationToken ct)
    {
        var project = await projects.CreateAsync(input, ct);
        return CreatedAtAction(nameof(Get), new { id = project.Id }, project);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectDto>> Update(int id, ProjectInput input, CancellationToken ct)
        => await projects.UpdateAsync(id, input, ct) is { } project ? Ok(project) : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await projects.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
