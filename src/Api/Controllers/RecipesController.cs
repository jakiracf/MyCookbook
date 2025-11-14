using Microsoft.AspNetCore.Mvc;
using MyCookbook.Application.Recipes;

namespace MyCookbook.Api.Controllers;

//endpoints

[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _service;
    private readonly ILogger<RecipesController> _log;

    public RecipesController(IRecipeService service, ILogger<RecipesController> log)
    {
        _service = service;
        _log = log;
    }

    // Example: GET /api/recipes?page=1&pageSize=20&sort=CreatedAtDesc
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RecipeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RecipeSummaryDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] RecipeSort sort = RecipeSort.CreatedAtDesc,
        CancellationToken ct = default
    )
    {
        //get pageSize from config instead of HC==1
        var result = await _service.ListAsync(page, pageSize, sort, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateManual(
        [FromBody] RecipeCreateDto dto,
        CancellationToken ct = default
    )
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var id = await _service.CreateManualAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecipeDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var recipe = await _service.GetAsync(id, ct);
        if (recipe is null)
            return NotFound();
        return Ok(recipe);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ExternalSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ExternalSearchResultDto>>> Search(
        [FromQuery] string query,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrEmpty(query))
            return BadRequest("query is required");
        var results = await _service.SearchExternalAsync(query, ct);
        return Ok(results);
    }

    [HttpPost("from-external")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImportFromExtenal(
        [FromBody] ImportFromExternalRequest body,
        CancellationToken ct = default
    )
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        try
        {
            var result = await _service.SaveFromExternalAsync(body.ExternalId, ct);
            if (result.Created)
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    new { id = result.Id }
                );
            return Ok(new { id = result.Id });
        }
        catch (KeyNotFoundException ex)
        {
            _log.LogError(ex, "External recipe not found");
            return NotFound(new { message = ex.Message });
        }
    }

    public sealed record ImportFromExternalRequest(
        [param: System.ComponentModel.DataAnnotations.Required] string ExternalId
    );
}
