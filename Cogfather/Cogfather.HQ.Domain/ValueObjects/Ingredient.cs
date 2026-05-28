namespace Cogfather.HQ.Domain.ValueObjects;

/// <summary>An input component required to craft a recipe, with its required quantity.</summary>
/// <param name="ComponentId">Component identifier (matches a recipe ID if craftable).</param>
/// <param name="Amount">Units required per craft cycle.</param>
public record Ingredient(string ComponentId, double Amount);