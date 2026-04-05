namespace Cogfather.Contracts.Models;

public record RecipeDto(
    string Id, 
    double Energy, 
    IEnumerable<IngredientDto> Ingredients, 
    IEnumerable<IngredientDto> Products
);