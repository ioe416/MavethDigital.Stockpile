using Stockpile.Application.Purchasing;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Tests.Fakes
{
    public sealed class FakePurchaseRepository
        : IPurchaseRepository
    {
        public Purchase? Purchase { get; set; }

        public Task<Purchase?> GetByIdAsync(
            Guid purchaseId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Purchase);
        }

    }
}
