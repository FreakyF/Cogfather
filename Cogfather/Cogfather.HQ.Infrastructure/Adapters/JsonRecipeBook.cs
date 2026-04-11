using System.Text.Json;
using Cogfather.Contracts.Models;
using Cogfather.HQ.Domain.Interfaces;
using Cogfather.HQ.Domain.ValueObjects;
using Microsoft.Extensions.Hosting;

namespace Cogfather.HQ.Infrastructure.Adapters;

public class JsonRecipeBook : IRecipeBook
{
    private readonly string _filePath;
    private List<Recipe>? _recipes;

    public JsonRecipeBook(IHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, "recipes.json");
    }

    public async Task<Recipe?> GetRecipeAsync(string recipeId)
    {
        await EnsureLoadedAsync();
        return _recipes?.FirstOrDefault(r => r.Id == recipeId);
    }

    public async Task<IEnumerable<Recipe>> GetAllRecipesAsync()
    {
        await EnsureLoadedAsync();
        return _recipes ?? Enumerable.Empty<Recipe>();
    }

    private async Task EnsureLoadedAsync()
    {
        if (_recipes != null) return;

        if (!File.Exists(_filePath))
        {
            _recipes = new List<Recipe>();
            return;
        }

        var json = await File.ReadAllTextAsync(_filePath);
        var dtos = JsonSerializer.Deserialize<List<RecipeDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        _recipes = dtos?.Select(dto => new Recipe(
            dto.Id,
            dto.Energy,
            dto.Ingredients.Select(i => new Ingredient(i.Id, i.Amount)).ToList(),
            dto.Products.Select(p => new Product(p.Id, p.Amount)).ToList()
        )).ToList() ?? new List<Recipe>();
    }
}