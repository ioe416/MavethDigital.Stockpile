using MavethDigital.Forge.Core.Models;


namespace Stockpile.Domain.Purchasing.Models;

public sealed class ReceiptLine : Entity
{
    public Guid PurchaseLineId { get; }

    public int QuantityReceived { get; private set; }

    public ReceiptLine(
        Guid purchaseLineId,
        int quantityReceived,
        DateTimeOffset createdAt) 
        : base (createdAt)
    {
        if (purchaseLineId == Guid.Empty)
            throw new ArgumentException(
                "A valid purchasLine is required",
                nameof(purchaseLineId));

        if (quantityReceived <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantityReceived));

        PurchaseLineId = purchaseLineId;
        QuantityReceived = quantityReceived;
    }
}
