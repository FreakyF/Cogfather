using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Application.Interfaces;

/// <summary>
/// Read-only access to the recipe catalogue loaded from the recipe book file.
/// </summary>
public interface IProductionCatalog
{
    /// <summary>
    /// Returns the <see cref="Recipe"/> with the given <paramref name="recipeId"/>,
    /// or <see langword="null"/> if no such recipe exists.
    /// </summary>
    Task<Recipe?> GetRecipeAsync(string recipeId, CancellationToken cancellationToken = default);

    /// <summary>Returns every recipe available in the catalogue.</summary>
    Task<IEnumerable<Recipe>> GetAllRecipesAsync(CancellationToken cancellationToken = default);
}
