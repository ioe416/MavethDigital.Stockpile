using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Purchasing;

public interface IReceiptRepository
{
    Task AddAsync(
        Receipt receipt,
        CancellationToken cancellationToken = default);
}
