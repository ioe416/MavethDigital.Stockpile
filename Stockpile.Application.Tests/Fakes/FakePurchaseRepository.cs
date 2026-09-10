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

        public Task<Purchase?> RecordReceipt(
            Guid purchaseId, 
            Guid purchaseLineId, 
            int quantityReceived, 
            DateTimeOffset createdAt, 
            CancellationToken cancellationToken = default)
        {
            if (Purchase == null)
                throw new InvalidOperationException("Purchase not found");

            Purchase.RecordReceipt(
                createdAt, 
                purchaseLineId, 
                quantityReceived);

            return Task.FromResult<Purchase?>(Purchase);
        }
    }
}
