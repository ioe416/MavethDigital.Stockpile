using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Purchasing;

public interface IReceiptRepository
{
    Task AddAsync(
        Receipt receipt,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Receipt receipt,
        CancellationToken cancellationToken = default);

    Task<Receipt?> GetByPurchaseIdAsync(
        Guid purchaseId,
        CancellationToken cancellationToken = default);
}
