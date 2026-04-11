using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Domain.Interfaces;

public interface IRecipeBook
{
    Task<Recipe?> GetRecipeAsync(string recipeId);
    Task<IEnumerable<Recipe>> GetAllRecipesAsync();
}