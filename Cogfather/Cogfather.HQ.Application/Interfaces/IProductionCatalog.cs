using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Application.Interfaces;

public interface IProductionCatalog
{
    Task<Recipe?> GetRecipeAsync(string recipeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Recipe>> GetAllRecipesAsync(CancellationToken cancellationToken = default);
}