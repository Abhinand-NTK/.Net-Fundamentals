using Microsoft.AspNetCore.Mvc;
using PersonalDetailsApi.Contracts.Requests;
using PersonalDetailsApi.Contracts.Responses;
using PersonalDetailsApi.Services;

namespace PersonalDetailsApi.Controllers;

/// <summary>
/// Create, read, update and delete personal records, including addresses, emergency contacts,
/// education, employment history and identity documents.
/// </summary>
[ApiController]
[Route("api/v1/persons")]
[Produces("application/json")]
[Tags("Persons")]
public sealed class PersonsController(IPersonService personService) : ControllerBase
{
    /// <summary>Search persons.</summary>
    /// <remarks>
    /// Supports free-text search, filtering by gender / blood group / marital status / city / age range,
    /// sorting and paging. Example: <c>GET /api/v1/persons?search=men&amp;minAge=25&amp;sortBy=DateOfBirth&amp;sortDirection=Desc</c>
    /// </remarks>
    /// <response code="200">A page of matching persons.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet]
    [ProducesResponseType<PagedResult<PersonSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<PersonSummaryResponse>>> Search(
        [FromQuery] PersonQueryParameters query, CancellationToken ct) =>
        Ok(await personService.SearchAsync(query, ct));

    /// <summary>Get one person with all details.</summary>
    /// <param name="id">The person's id.</param>
    /// <param name="ct">Cancellation token (supplied by ASP.NET Core).</param>
    /// <response code="200">The person.</response>
    /// <response code="404">No person with this id.</response>
    [HttpGet("{id:guid}", Name = nameof(GetPersonById))]
    [ProducesResponseType<PersonDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonDetailResponse>> GetPersonById(Guid id, CancellationToken ct) =>
        Ok(await personService.GetByIdAsync(id, ct));

    /// <summary>Create a person.</summary>
    /// <response code="201">Created; the Location header points to the new resource.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="409">The e-mail address is already in use.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<PersonDetailResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PersonDetailResponse>> Create([FromBody] PersonRequest request, CancellationToken ct)
    {
        var created = await personService.CreateAsync(request, ct);
        return CreatedAtRoute(nameof(GetPersonById), new { id = created.Id }, created);
    }

    /// <summary>Replace a person.</summary>
    /// <remarks>Full replacement: every field and every child collection is overwritten by the request body.</remarks>
    /// <response code="200">Updated person.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">No person with this id.</response>
    /// <response code="409">The e-mail address is already in use by another person.</response>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType<PersonDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PersonDetailResponse>> Update(Guid id, [FromBody] PersonRequest request, CancellationToken ct) =>
        Ok(await personService.UpdateAsync(id, request, ct));

    /// <summary>Delete a person.</summary>
    /// <remarks>Soft delete: the record is hidden from the API but kept in the database.</remarks>
    /// <response code="204">Deleted.</response>
    /// <response code="404">No person with this id.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await personService.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Aggregated statistics.</summary>
    /// <remarks>Counts by gender, blood group, nationality and age group, plus the average age.</remarks>
    /// <response code="200">Statistics.</response>
    [HttpGet("statistics")]
    [ProducesResponseType<PersonStatisticsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PersonStatisticsResponse>> GetStatistics(CancellationToken ct) =>
        Ok(await personService.GetStatisticsAsync(ct));
}
