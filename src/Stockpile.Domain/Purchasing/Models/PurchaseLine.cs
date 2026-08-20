using MavethDigital.Forge.Core.Models;
using MavethDigital.Forge.Core.ValueObjects;

namespace Stockpile.Domain.Purchasing.Models;

public sealed class PurchaseLine : Entity
{
    public Guid PartId { get; }
    
    public int Quantity { get; }
    
    public Money? UnitPrice { get; }

    public PurchaseLine(
        Guid partId,
        int quantity,
        DateTimeOffset createdAt,
        Money? unitPrice = null
        ) : base (createdAt)
    {
        if (partId == Guid.Empty)
            throw new ArgumentException("A valid part is required.", nameof(partId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                 nameof(quantity),
                "A quantity greater than 0 is required.");

        PartId = partId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

}
