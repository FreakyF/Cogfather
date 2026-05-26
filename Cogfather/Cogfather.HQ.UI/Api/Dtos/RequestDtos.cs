namespace Cogfather.HQ.UI.Api.Dtos;

public sealed record IssueOrderRequest(string RecipeId, double RequestedQuantity);

public sealed record SetFaultRequest(string FaultMode, int DelaySeconds = 0);
