using Stockpile.Application.Purchasing;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Tests.Fakes
{
    public sealed class FakeReceiptRepository
        : IReceiptRepository
    {
        public Receipt? AddedReceipt { get; private set; }

        public Task AddAsync(
            Receipt receipt,
            CancellationToken cancellationToken = default)
        {
            AddedReceipt = receipt;
            return Task.CompletedTask;
        }
    }
}
