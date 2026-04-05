using Cogfather.HQ.Domain.ValueObjects;

namespace Cogfather.HQ.Domain.Entities;

public class Recipe
{
    public string Id { get; }
    public double Energy { get; }
    public IReadOnlyList<ProductionItem> Ingredients { get; }
    public IReadOnlyList<ProductionItem> Products { get; }

    public Recipe(string id, double energy, IEnumerable<ProductionItem> ingredients, IEnumerable<ProductionItem> products)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Recipe ID cannot be empty.", nameof(id));
        
        Id = id;
        Energy = energy;
        Ingredients = ingredients.ToList().AsReadOnly();
        Products = products.ToList().AsReadOnly();
    }
}