public sealed record ReplenishmentItemDto(
    Guid PartId,
    int OnHandQuantity,
    int AllocatedQuantity,
    int AvailableQuantity,
    int MinimumQuantity,
    int ReorderQuantity);