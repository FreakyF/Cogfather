using Cogfather.HQ.Application.Queries.GetInventory;
using Cogfather.HQ.UI.Api.Dtos;
using MediatR;

namespace Cogfather.HQ.UI.Api;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/inventory", GetInventoryAsync)
            .RequireAuthorization()
            .WithTags("Inventory")
            .WithName("GetInventory")
            .Produces<object>()
            .ProducesProblem(401);

        return app;
    }

    private static async Task<IResult> GetInventoryAsync(ISender sender, CancellationToken ct = default)
    {
        var inventory = await sender.Send(new GetInventoryQuery(), ct);
        var items = inventory.Items
            .Select(kvp => new InventoryItemDto(kvp.Key, kvp.Value))
            .OrderBy(i => i.ItemId)
            .ToList();

        return Results.Ok(new { items });
    }
}
