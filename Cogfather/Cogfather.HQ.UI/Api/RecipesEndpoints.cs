using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.UI.Api.Dtos;

namespace Cogfather.HQ.UI.Api;

public static class RecipesEndpoints
{
    public static IEndpointRouteBuilder MapRecipesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/recipes", GetRecipesAsync)
            .RequireAuthorization()
            .WithTags("Recipes")
            .WithName("GetRecipes")
            .Produces<object>()
            .ProducesProblem(401);

        return app;
    }

    private static async Task<IResult> GetRecipesAsync(
        IProductionCatalog catalog,
        CancellationToken ct = default)
    {
        var recipes = await catalog.GetAllRecipesAsync(ct);
        var dtos = recipes.Select(r => new RecipeResponseDto(
            r.Id,
            r.Energy,
            r.Ingredients.Select(i => new IngredientResponseDto(i.ComponentId, i.Amount)).ToList(),
            r.Products.Select(p => new ProductResponseDto(p.ComponentId, p.Amount)).ToList()
        )).ToList();

        return Results.Ok(new { recipes = dtos });
    }
}
