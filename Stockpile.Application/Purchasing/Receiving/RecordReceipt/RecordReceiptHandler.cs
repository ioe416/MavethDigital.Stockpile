
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Purchasing.Receiving.RecordReceipt
{
    public sealed class RecordReceiptHandler
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IReceiptRepository _receiptRepository;

        public RecordReceiptHandler(
            IPurchaseRepository purchaseRepository,
            IReceiptRepository receiptRepository)
        {
            _purchaseRepository = purchaseRepository;
            _receiptRepository = receiptRepository;
        }

        public async Task<RecordReceiptResult> HandleAsync(
            RecordReceiptCommand command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            var purchase = await _purchaseRepository.GetByIdAsync(
                command.PurchaseId,
                cancellationToken);

            if (purchase == null)
                throw new InvalidOperationException("Purchase not found");

            var purchaseLine = purchase.Lines.FirstOrDefault
                (line => line.Id == command.PurchaseLineId);

            if (purchaseLine == null)
                throw new InvalidOperationException("Purchase line not found");

            var receipt = new Receipt(
                command.PurchaseId,
                command.CreatedAt);

            var receiptLine = new ReceiptLine(
                command.PurchaseLineId,
                command.QuantityReceived,
                command.CreatedAt);

            if (purchase.Lines.All(line => line.Id != command.PurchaseLineId))
                throw new InvalidOperationException("Purchase line not found");

            receipt.AddLine(command.CreatedAt, receiptLine);

            await _receiptRepository.AddAsync(
                receipt,
                cancellationToken);

            return new RecordReceiptResult(receipt.Id);
        }

    }
}
