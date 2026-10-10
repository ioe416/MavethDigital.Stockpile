
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Purchasing.Receiving.RecordReceipt
{
    public sealed class RecordReceiptHandler
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IReceiptRepository _receiptRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public RecordReceiptHandler(
            IPurchaseRepository purchaseRepository,
            IReceiptRepository receiptRepository,
            IInventoryRepository inventoryRepository)
        {
            _purchaseRepository = purchaseRepository;
            _receiptRepository = receiptRepository;
            _inventoryRepository = inventoryRepository;
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

            receipt.AddLine(command.CreatedAt, receiptLine);

            await _receiptRepository.AddAsync(
                receipt,
                cancellationToken);

            await _purchaseRepository.RecordReceipt(
                purchase.Id,
                command.PurchaseLineId,
                command.QuantityReceived,
                command.CreatedAt,
                cancellationToken);

            var inventory =
                await _inventoryRepository.GetByPartIdAsync(
                    purchaseLine.PartId,
                    cancellationToken);

            if (inventory is null)
            {
                throw new InvalidOperationException(
                    $"Inventory item not found for part {purchaseLine.PartId}");
            }

            inventory.Receive(
                command.QuantityReceived,
                command.CreatedAt);

            await _inventoryRepository.UpdateAsync(
                inventory,
                cancellationToken);

            return new RecordReceiptResult(receipt.Id);
        }

    }
}
