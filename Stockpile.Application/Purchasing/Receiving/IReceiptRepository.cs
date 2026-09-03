
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Purchasing.Receiving;

public interface IReceiptRepository
{
    Task <Receipt?> GetByIdAsync(
        Guid receiptId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Receipt receipt,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Receipt receipt,
        CancellationToken cancellationToken = default);
}
