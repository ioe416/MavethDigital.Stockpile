using Stockpile.Domain.Inventory.Models;

internal sealed class FakeInventoryRepository : IInventoryRepository
{
    private readonly IReadOnlyCollection<InventoryItem> _items;

    public FakeInventoryRepository(
        IReadOnlyCollection<InventoryItem> items)
    {
        _items = items;
    }

    public Task<IReadOnlyCollection<InventoryItem>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_items);
    }
}