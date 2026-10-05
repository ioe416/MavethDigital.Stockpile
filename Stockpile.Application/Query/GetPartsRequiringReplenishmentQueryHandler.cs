using Stockpile.Application.Query;

public sealed class GetPartsRequiringReplenishmentQueryHandler
{
    private readonly IInventoryRepository _repository;

    public GetPartsRequiringReplenishmentQueryHandler(
        IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<ReplenishmentItemDto>> Handle(
        GetPartsRequiringReplenishmentQuery query,
        CancellationToken cancellationToken)
    {
        var inventoryItems =
            await _repository.GetAllAsync(cancellationToken);

        return inventoryItems
            .Where(x => x.NeedsReplenishment)
            .Select(x => new ReplenishmentItemDto(
                x.PartId,
                x.OnHandQuantity,
                x.AllocatedQuantity,
                x.AvailableQuantity,
                x.MinimumQuantity,
                x.ReorderQuantity))
            .ToList();
    }
}