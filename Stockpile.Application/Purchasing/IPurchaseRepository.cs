using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Purchasing;

public interface IPurchaseRepository
{
    Task<Purchase?> GetByIdAsync(
    Guid purchaseId,
    CancellationToken cancellationToken = default);

    Task<Purchase?> RecordReceipt(
        Guid purchaseId,
        Guid purchaseLineId,
        int quantityReceived,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken = default);

    Task<Purchase> UpdateAsync(
        Purchase purchase,
        CancellationToken cancellationToken = default);

}


