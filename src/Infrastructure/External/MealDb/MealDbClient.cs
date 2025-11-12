using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using myCookbook.Infrastructure.External.MealDb;
using MyCookbook.Application.Recipes;
using static MyCookbook.Application.Recipes.IExternalRecipeClient;

namespace MyCookbook.Infrastructure.External.MealDb;

public sealed class MealDbClient : IExternalRecipeClient
{
    private readonly HttpClient _http;
    private readonly MealDbOptions _opt;
    private readonly ILogger<MealDbClient> _log;

    public MealDbClient(HttpClient http, IOptions<MealDbOptions> opt, ILogger<MealDbClient> log)
    {
        _http = http;
        _opt = opt.Value;
        _log = log;
    }

    public async Task<IReadOnlyList<ExternalRecipeDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("query is required", nameof(query));

        var q = Uri.EscapeDataString(query.Trim());
        var url = $"{_opt.BaseUrl}/{_opt.ApiKey}/search.php?s={q}";

        _log.LogInformation("MealDB: GET {Url}", url);

        //var resp = await _http.GetFromJsonAsync<MealDbSearchResponse>(url, ct);
        try
        {
            // 1) do the HTTP call
            using var resp = await _http.GetAsync(url, ct);

            // 2) handle non-success status codes
            if (!resp.IsSuccessStatusCode)
            {
                _log.LogWarning("MealDB non-success {Status} for {Url}", (int)resp.StatusCode, url);
                return Array.Empty<ExternalRecipeDto>();
            }

            // 3) deserialize
            var model = await resp.Content.ReadFromJsonAsync<MealDbSearchResponse>(cancellationToken: ct);
            if (model?.Meals is null || model.Meals.Count == 0)
                return Array.Empty<ExternalRecipeDto>();

            // 4) map
            var results = new List<ExternalRecipeDto>(model.Meals.Count);
            foreach (var m in model.Meals)
            {
                var ingredients = CollectIngredients(m);
                results.Add(new ExternalRecipeDto(
                    ExternalId: m.idMeal,
                    Name: m.strMeal,
                    Instructions: m.strInstructions,
                    ImageUrl: m.strMealThumb,
                    Ingredients: ingredients
                ));
            }

            _log.LogInformation("MealDB returned {Count} results for '{Query}'", results.Count, query);
            return results;
        }
        catch (TaskCanceledException) when (ct.IsCancellationRequested)
        {
            _log.LogWarning("MealDB cancelled for '{Query}'", query);
            return Array.Empty<ExternalRecipeDto>();
        }
        catch (HttpRequestException ex)
        {
            _log.LogWarning(ex, "MealdbHttpRequestException for '{Query}'", query);
            return Array.Empty<ExternalRecipeDto>();
        }
        catch (JsonException ex)
    {
        _log.LogWarning(ex, "MealDB JSON parse error for '{Query}'", query);
        return Array.Empty<ExternalRecipeDto>();
    }

    }

    private static IReadOnlyList<ExternalIngredient> CollectIngredients(MealDbMeal m)
    {
        var list = new List<ExternalIngredient>(20);
        var extra = m.Extra;
        if (extra is null) return list;

        for (int i = 1; i <= 20; i++)
        {
            if (!extra.TryGetValue($"strIngredient{i}", out var ingEl)) continue;
            var name = ingEl.ValueKind == JsonValueKind.String ? ingEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(name)) continue;

            string? measure = null;
            if (extra.TryGetValue($"strMeasure{i}", out var measEl) && measEl.ValueKind == JsonValueKind.String)
            {
                var raw = measEl.GetString();
                measure = string.IsNullOrWhiteSpace(raw) ? null : raw!.Trim();
            }

            list.Add(new ExternalIngredient(name.Trim(), measure));
        }
        return list;
    }
}
