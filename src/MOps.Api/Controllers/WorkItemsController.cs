using Microsoft.AspNetCore.Mvc;
using MOps.Core.Dtos;
using MOps.Core.Services;

namespace MOps.Api.Controllers;

[ApiController]
public sealed class WorkItemsController(IWorkItemService workItems) : ControllerBase
{
    [HttpGet("api/projects/{projectId:int}/work-items")]
    public async Task<ActionResult<IReadOnlyList<WorkItemDto>>> List(int projectId, [FromQuery] WorkItemQuery query, CancellationToken ct)
        => await workItems.ListAsync(projectId, query, ct) is { } list ? Ok(list) : NotFound();

    [HttpPost("api/projects/{projectId:int}/work-items")]
    public async Task<ActionResult<WorkItemDto>> Create(int projectId, WorkItemInput input, CancellationToken ct)
        => await workItems.CreateAsync(projectId, input, ct) is { } item
            ? CreatedAtAction(nameof(Get), new { id = item.Id }, item)
            : NotFound();

    [HttpGet("api/work-items/{id:int}")]
    public async Task<ActionResult<WorkItemDto>> Get(int id, CancellationToken ct)
        => await workItems.GetAsync(id, ct) is { } item ? Ok(item) : NotFound();

    [HttpPut("api/work-items/{id:int}")]
    public async Task<ActionResult<WorkItemDto>> Update(int id, WorkItemInput input, CancellationToken ct)
        => await workItems.UpdateAsync(id, input, ct) is { } item ? Ok(item) : NotFound();

    [HttpDelete("api/work-items/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await workItems.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
