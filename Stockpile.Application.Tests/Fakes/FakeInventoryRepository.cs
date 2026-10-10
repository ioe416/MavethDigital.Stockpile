using Stockpile.Domain.Inventory.Models;

internal sealed class FakeInventoryRepository
    : IInventoryRepository
{
    private readonly List<InventoryItem> _items;

    public FakeInventoryRepository(
        IReadOnlyCollection<InventoryItem> items)
    {
        _items = items.ToList();
    }

    public Task<IReadOnlyCollection<InventoryItem>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<InventoryItem>>(
            _items);
    }

    public Task<InventoryItem?> GetByPartIdAsync(
        Guid partId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            _items.SingleOrDefault(x => x.PartId == partId));
    }

    public Task UpdateAsync(
        InventoryItem inventoryItem,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
