using Cogfather.Contracts.Enums;
using Cogfather.HQ.Application.Commands.ClearNodeFault;
using Cogfather.HQ.Application.Commands.SetNodeFault;
using Cogfather.HQ.Application.Queries.GetAllNodes;
using Cogfather.HQ.Domain.Enums;
using Cogfather.HQ.Infrastructure.Messaging;
using Cogfather.HQ.UI.Api.Dtos;
using MediatR;

namespace Cogfather.HQ.UI.Api;

public static class NodesEndpoints
{
    public static IEndpointRouteBuilder MapNodesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/nodes")
            .RequireAuthorization()
            .WithTags("Nodes");

        group.MapGet("/", GetNodesAsync)
            .WithName("GetNodes")
            .Produces<object>()
            .ProducesProblem(401);

        group.MapPost("/{nodeId}/fault", SetFaultAsync)
            .WithName("SetFault")
            .Produces<FaultResponseDto>()
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(401);

        group.MapDelete("/{nodeId}/fault", ClearFaultAsync)
            .WithName("ClearFault")
            .Produces<FaultResponseDto>()
            .ProducesProblem(404)
            .ProducesProblem(401);

        return app;
    }

    private static async Task<IResult> GetNodesAsync(ISender sender, CancellationToken ct = default)
    {
        var nodes = await sender.Send(new GetAllNodesQuery(), ct);
        var dtos = nodes.Select(n => new NodeDto(n.NodeId, n.Address, n.Status.ToString(), n.FaultMode.ToString()));
        return Results.Ok(new { nodes = dtos });
    }

    private static async Task<IResult> SetFaultAsync(
        string nodeId,
        SetFaultRequest request,
        ISender sender,
        FaultControlPublisher faultPublisher,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<FaultModeContract>(request.FaultMode, ignoreCase: true, out var contractMode))
            return Results.Problem(title: "Bad Request", statusCode: 400, detail: $"Unknown fault mode: {request.FaultMode}");

        var hqMode = contractMode == FaultModeContract.None ? FaultMode.None : FaultMode.Byzantine;

        try
        {
            await sender.Send(new SetNodeFaultCommand(nodeId, hqMode), ct);
            await faultPublisher.PublishAsync(nodeId, contractMode, request.DelaySeconds, ct);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.Problem(title: "Not Found", statusCode: 404, detail: ex.Message);
        }

        return Results.Ok(new FaultResponseDto(nodeId, contractMode.ToString()));
    }

    private static async Task<IResult> ClearFaultAsync(
        string nodeId,
        ISender sender,
        FaultControlPublisher faultPublisher,
        CancellationToken ct = default)
    {
        try
        {
            await sender.Send(new ClearNodeFaultCommand(nodeId), ct);
            await faultPublisher.PublishAsync(nodeId, FaultModeContract.None, 0, ct);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.Problem(title: "Not Found", statusCode: 404, detail: ex.Message);
        }

        return Results.Ok(new FaultResponseDto(nodeId, FaultModeContract.None.ToString()));
    }
}
