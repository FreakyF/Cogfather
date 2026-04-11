namespace Cogfather.HQ.Domain.ValueObjects;

public record Recipe
{
    public Recipe(string id, double energy, IEnumerable<Ingredient> ingredients, IEnumerable<Product> products)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Recipe ID cannot be empty.", nameof(id));

        Id = id;
        Energy = energy;
        Ingredients = ingredients.ToList().AsReadOnly();
        Products = products.ToList().AsReadOnly();
    }

    public string Id { get; init; }
    public double Energy { get; init; }
    public IReadOnlyList<Ingredient> Ingredients { get; init; }
    public IReadOnlyList<Product> Products { get; init; }
}