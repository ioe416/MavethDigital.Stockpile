using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Purchasing
{
    public interface IPurchaseRepository
    {
        Task<Purchase?> GetByIdAsync(
        Guid purchaseId,
        CancellationToken cancellationToken = default);

    }
}
