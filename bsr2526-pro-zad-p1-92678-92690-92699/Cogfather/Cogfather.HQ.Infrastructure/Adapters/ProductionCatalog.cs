using System.Text.Json;
using System.Text.Json.Serialization;
using Cogfather.HQ.Application.Interfaces;
using Cogfather.HQ.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace Cogfather.HQ.Infrastructure.Adapters;

internal class JsonIngredientDto
{
    [JsonPropertyName("name")]
    public string Id { get; set; } = string.Empty;
    public double Amount { get; set; }
}

internal class JsonRecipeDto
{
    public double Energy { get; set; }

    [JsonConverter(typeof(EmptyObjectOrArrayConverter<List<JsonIngredientDto>>))]
    public List<JsonIngredientDto>? Ingredients { get; set; }

    [JsonConverter(typeof(EmptyObjectOrArrayConverter<List<JsonIngredientDto>>))]
    public List<JsonIngredientDto>? Products { get; set; }
}

public class EmptyObjectOrArrayConverter<T> : JsonConverter<T> where T : class, new()
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) return JsonSerializer.Deserialize<T>(ref reader, options);
        using var doc = JsonDocument.ParseValue(ref reader);
        return new T();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}

[JsonSerializable(typeof(Dictionary<string, JsonRecipeDto>))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
internal partial class CatalogJsonContext : JsonSerializerContext
{
}

public class ProductionCatalog : IProductionCatalog
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private Dictionary<string, Recipe>? _cache;

    public ProductionCatalog(IConfiguration configuration)
    {
        var relativePath = configuration["Catalog:RecipesFilePath"]
                           ?? throw new InvalidOperationException("Missing Catalog:RecipesFilePath");
        _filePath = Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    public async Task<Recipe?> GetRecipeAsync(string recipeId, CancellationToken ct = default)
    {
        var catalog = await EnsureLoadedAsync(ct);
        return catalog.GetValueOrDefault(recipeId);
    }

    public async Task<IEnumerable<Recipe>> GetAllRecipesAsync(CancellationToken ct = default)
    {
        var catalog = await EnsureLoadedAsync(ct);
        return catalog.Values;
    }

    private async Task<Dictionary<string, Recipe>> EnsureLoadedAsync(CancellationToken ct)
    {
        if (_cache != null) return _cache;
        await _semaphore.WaitAsync(ct);
        try
        {
            if (_cache != null) return _cache;
            if (!File.Exists(_filePath)) throw new FileNotFoundException(_filePath);

            await using var stream = File.OpenRead(_filePath);

            var rawData = await JsonSerializer.DeserializeAsync(
                stream,
                CatalogJsonContext.Default.DictionaryStringJsonRecipeDto,
                ct);

            _cache = rawData?
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => new Recipe(
                        kvp.Key,
                        kvp.Value.Energy,
                        kvp.Value.Ingredients?.Select(i => new Ingredient(i.Id, i.Amount)) ?? [],
                        kvp.Value.Products?.Select(p => new Product(p.Id, p.Amount)) ?? []
                    )
                ) ?? [];

            return _cache;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}