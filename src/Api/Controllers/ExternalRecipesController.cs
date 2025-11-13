using Microsoft.AspNetCore.Mvc;
using MyCookbook.Application.Recipes;

namespace MyCookbook.Api.Controllers;

[ApiController]
[Route("api/external-recipes")]
public class ExternalRecipesController : ControllerBase
{
    private readonly IExternalRecipeClient _client;

    public ExternalRecipesController(IExternalRecipeClient client) => _client = client;

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        CancellationToken ct = default
    )
    {
        //add<ed> cancelation token after error fixes
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("query is required");
        var results = await _client.SearchAsync(query, ct);
        return Ok(results);
    }
}
