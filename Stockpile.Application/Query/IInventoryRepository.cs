using Stockpile.Domain.Inventory.Models;

public interface IInventoryRepository
{
    Task<IReadOnlyCollection<InventoryItem>> GetAllAsync(
        CancellationToken cancellationToken);
}