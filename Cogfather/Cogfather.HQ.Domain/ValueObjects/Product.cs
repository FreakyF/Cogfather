namespace Cogfather.HQ.Domain.ValueObjects;

/// <summary>An output component produced by a recipe, with its yield per craft cycle.</summary>
/// <param name="ComponentId">Component identifier of the produced item.</param>
/// <param name="Amount">Units produced per craft cycle.</param>
public record Product(string ComponentId, double Amount);