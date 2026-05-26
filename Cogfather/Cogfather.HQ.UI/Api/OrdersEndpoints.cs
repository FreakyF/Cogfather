using Cogfather.HQ.Application.Commands.IssueProductionOrder;
using Cogfather.HQ.Application.Queries.GetOrderById;
using Cogfather.HQ.Application.Queries.GetOrders;
using Cogfather.HQ.Application.Queries.GetReportsByRecipeId;
using Cogfather.HQ.UI.Api.Dtos;
using MediatR;

namespace Cogfather.HQ.UI.Api;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .RequireAuthorization()
            .WithTags("Orders");

        group.MapGet("/", GetOrdersAsync)
            .WithName("GetOrders")
            .Produces<PagedResponse<OrderSummaryDto>>()
            .ProducesProblem(401);

        group.MapGet("/{id:guid}", GetOrderByIdAsync)
            .WithName("GetOrderById")
            .Produces<OrderDetailDto>()
            .ProducesProblem(404)
            .ProducesProblem(401);

        group.MapPost("/", IssueOrderAsync)
            .WithName("IssueOrder")
            .Produces<IssueOrderResponse>(201)
            .ProducesProblem(400)
            .ProducesProblem(401);

        return app;
    }

    private static async Task<IResult> GetOrdersAsync(
        ISender sender,
        string? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var orders = await sender.Send(new GetOrdersQuery(), ct);

        var filtered = status is null
            ? orders
            : orders.Where(o => o.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase));

        var ordered = filtered.OrderByDescending(o => o.CreatedAt).ToList();
        var total = ordered.Count;
        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderSummaryDto(o.Id, o.RecipeId, o.TargetAmount, o.Status.ToString(), o.CreatedAt))
            .ToList();

        return Results.Ok(new PagedResponse<OrderSummaryDto>(items, total, page, pageSize));
    }

    private static async Task<IResult> GetOrderByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken ct = default)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id), ct);
        if (order is null)
            return Results.Problem(title: "Not Found", statusCode: 404, detail: "Order not found.");

        var reports = await sender.Send(new GetReportsByRecipeIdQuery(order.RecipeId), ct);
        var reportDtos = reports
            .Select(r => new ReportSummaryDto(r.Id, r.NodeId, r.RecipeId, r.Success, r.ReportedAt))
            .ToList();

        var dto = new OrderDetailDto(
            order.Id,
            order.RecipeId,
            order.TargetAmount,
            order.Status.ToString(),
            order.CreatedAt,
            reportDtos);

        return Results.Ok(dto);
    }

    private static async Task<IResult> IssueOrderAsync(
        IssueOrderRequest request,
        ISender sender,
        CancellationToken ct = default)
    {
        try
        {
            var orderId = await sender.Send(
                new IssueProductionOrderCommand(request.RecipeId, request.RequestedQuantity), ct);
            return Results.Created($"/api/v1/orders/{orderId}", new IssueOrderResponse(orderId));
        }
        catch (Exception ex) when (ex.Message.Contains("not found") || ex.Message.Contains("Recipe"))
        {
            return Results.Problem(title: "Bad Request", statusCode: 400, detail: ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return Results.Problem(
                title: "Validation Error",
                statusCode: 400,
                detail: string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
        }
    }
}
