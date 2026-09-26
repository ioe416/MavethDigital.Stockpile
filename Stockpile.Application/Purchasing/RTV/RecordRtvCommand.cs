
namespace Stockpile.Application.Purchasing.RTV;

public sealed record RecordRtvCommand(
    Guid PurchaseId,
    Guid PurchaseLineId,
    int QuantityToReturn,
    DateTimeOffset CreatedAt
    );
