using Stockpile.Application.Purchasing;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Tests.Fakes
{
    public sealed class FakeReceiptRepository
        : IReceiptRepository
    {
        public Receipt? AddedReceipt { get; private set; }

        private readonly List<Receipt> _receipts = [];

        public IReadOnlyCollection<Receipt> Receipts =>
            _receipts.AsReadOnly();

        public Task AddAsync(
            Receipt receipt,
            CancellationToken cancellationToken = default)
        {
            AddedReceipt = receipt;
            _receipts.Add(receipt);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            Receipt receipt,
            CancellationToken cancellationToken = default)
        {
            var existingReceipt = _receipts.FirstOrDefault(r => r.Id == receipt.Id);
            if (existingReceipt != null)
            {
                _receipts.Remove(existingReceipt);
                _receipts.Add(receipt);
            }
            return Task.CompletedTask;
        }

        public Task<Receipt?> GetByPurchaseIdAsync(
            Guid purchaseId,
            CancellationToken cancellationToken = default)
        {
            var receipt = _receipts.FirstOrDefault(r => r.PurchaseId == purchaseId);
            return Task.FromResult(receipt);
        }
    }
}
