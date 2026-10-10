using Stockpile.Domain.Inventory.Models;

public interface IInventoryRepository
{
    Task<IReadOnlyCollection<InventoryItem>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<InventoryItem?> GetByPartIdAsync(
        Guid partId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        InventoryItem inventoryItem,
        CancellationToken cancellationToken);
}