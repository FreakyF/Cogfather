namespace Cogfather.HQ.Domain.Exceptions;

public class RecipeNotFoundException : Exception
{
    public RecipeNotFoundException(string recipeId)
        : base($"Recipe with ID '{recipeId}' was not found.")
    {
        RecipeId = recipeId;
    }

    public string RecipeId { get; }
}