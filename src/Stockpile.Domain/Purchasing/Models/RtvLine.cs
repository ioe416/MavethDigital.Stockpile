
namespace Stockpile.Domain.Purchasing.Models;

public sealed class RtvLine
{
    public Guid Id { get; }
    public Guid PurchaseLineId { get; }
    public int QuantityReturned { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public RtvLine(Guid purchaseLineId, int quantityReturned, DateTimeOffset createdAt)
    {
        if (purchaseLineId == Guid.Empty)
            throw new ArgumentException("A valid purchase line is required",  nameof(purchaseLineId));
        if (quantityReturned <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantityReturned), "Quantity returned must be greater than 0");

        Id = Guid.NewGuid();
        PurchaseLineId = purchaseLineId;
        QuantityReturned = quantityReturned;
        UpdatedAt = createdAt;
    }
}
