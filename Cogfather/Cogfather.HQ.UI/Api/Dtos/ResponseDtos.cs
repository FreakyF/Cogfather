namespace Cogfather.HQ.UI.Api.Dtos;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record IssueOrderResponse(Guid OrderId);

public sealed record OrderSummaryDto(
    Guid Id,
    string RecipeId,
    double TargetAmount,
    string Status,
    DateTime CreatedAt);

public sealed record ConsensusResultDto(string RecipeId, string Verdict, double Accuracy);

public sealed record OrderDetailDto(
    Guid Id,
    string RecipeId,
    double TargetAmount,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<ReportSummaryDto> Reports);

public sealed record ReportSummaryDto(
    Guid Id,
    string NodeId,
    string RecipeId,
    bool Success,
    DateTime ReportedAt);

public sealed record NodeDto(
    string NodeId,
    string Address,
    string Status,
    string FaultMode);

public sealed record FaultResponseDto(string NodeId, string ActiveFault);

public sealed record InventoryItemDto(string ItemId, double Quantity);

public sealed record RecipeResponseDto(
    string Id,
    double Energy,
    IReadOnlyList<IngredientResponseDto> Ingredients,
    IReadOnlyList<ProductResponseDto> Products);

public sealed record IngredientResponseDto(string ComponentId, double Amount);

public sealed record ProductResponseDto(string ComponentId, double Amount);
