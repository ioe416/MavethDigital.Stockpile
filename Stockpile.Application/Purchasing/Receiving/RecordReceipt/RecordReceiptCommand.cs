
namespace Stockpile.Application.Purchasing.Receiving.RecordReceipt
{
    public sealed record RecordReceiptCommand(
        Guid PurchaseId,
        Guid PurchaseLineId,
        int QuantityReceived,
        DateTimeOffset CreatedAt); 
}
