using MavethDigital.Forge.Core.Models;
using MavethDigital.Forge.Core.ValueObjects;

namespace Stockpile.Domain.Purchasing.Models;

public sealed class PurchaseLine : Entity
{
    public Guid PartId { get; private set; }
    
    public int Quantity { get; private set; }

    public int ReceivedQuantity { get; set; } = 0;

    public int OutstandingQuantity => Quantity - ReceivedQuantity;

    public Money? UnitPrice { get; private set; }

    public bool IsPartiallyComplete =>
        ReceivedQuantity > 0 && ReceivedQuantity < Quantity;

    public PurchaseLine(
        Guid partId,
        int quantity,
        int receivedQuantity,
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
        ReceivedQuantity = receivedQuantity;
    }

    public void UpdateQuantity(DateTimeOffset updatedAt, 
        int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentOutOfRangeException(
                 "A quantity greater than 0 is required.");

        MarkUpdated(updatedAt);

        Quantity = newQuantity;
    }

    public void UpdateUnitPrice(DateTimeOffset updatedAt,
        Money newUnitPrice)
    {
        if (newUnitPrice.Amount < 0m)
            throw new ArgumentOutOfRangeException(
                 nameof(newUnitPrice),
                "A unit Price of $0.00 or greater is required.");

        MarkUpdated(updatedAt);

        UnitPrice = newUnitPrice;
    }

    public void UpdatePartId(DateTimeOffset updatedAt,
        Guid newPartId)
    {
        if (newPartId == PartId)
            return;

        if (newPartId == Guid.Empty)
            throw new ArgumentException(
                "A valid part is required");

        base.MarkUpdated(updatedAt);

        PartId = newPartId;
    }

    public void UpdateReceivedQuantity(DateTimeOffset updatedAt, int newReceivedQuantity)
    {
        if (newReceivedQuantity < 0)
            throw new ArgumentOutOfRangeException(
                 nameof(newReceivedQuantity),
                "A received quantity of 0 or greater is required.");

        if (newReceivedQuantity + ReceivedQuantity > Quantity)
            throw new InvalidOperationException(
               "Total received quantity cannot exceed ordered quantity.");

        MarkUpdated(updatedAt);
        ReceivedQuantity += newReceivedQuantity;
    }

}
