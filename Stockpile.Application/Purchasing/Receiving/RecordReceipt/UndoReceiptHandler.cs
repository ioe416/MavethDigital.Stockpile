
namespace Stockpile.Application.Purchasing.Receiving.RecordReceipt
{
    public sealed class UndoReceiptHandler
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IReceiptRepository _receiptRepository;

        public UndoReceiptHandler(
            IPurchaseRepository purchaseRepository,
            IReceiptRepository receiptRepository)
        {
            _purchaseRepository = purchaseRepository;
            _receiptRepository = receiptRepository;
        }
        public async Task<UndoReceiptResult> HandleAsync(
            UndoReceiptCommand command,
            CancellationToken cancellationToken = default)
        {
            var purchase = await _purchaseRepository.GetByIdAsync(command.PurchaseId, cancellationToken);
            if (purchase == null)
                return new UndoReceiptResult(false, "Purchase not found.");

            var receipt = await _receiptRepository.GetByPurchaseIdAsync(command.PurchaseId, cancellationToken);
            if ( receipt == null)
                return new UndoReceiptResult(false, "Receipt not found.");

            var receiptLine = receipt.Lines.SingleOrDefault(line => line.PurchaseLineId == command.PurchaseLineId);
            if (receiptLine == null)
                return new UndoReceiptResult(false, "Receipt line not found.");

            receipt.RemoveLine(command.CreatedAt, receiptLine.Id);

            purchase.UndoReceipt(command.PurchaseLineId, command.QuantityToUndo, command.CreatedAt);

            await _receiptRepository.UpdateAsync(receipt, cancellationToken);
            await _purchaseRepository.UpdateAsync(purchase, cancellationToken);

            return new UndoReceiptResult(true, "Receipt undone successfully");

        }
    }
}
