using MavethDigital.Forge.Domain.Models;
using Stockpile.Domain.Inventory.Enums;

public sealed class ReplenishmentRequest : AggregateRoot
{
    public ReplenishmentRequest(
        DateTimeOffset createdAt)
        : base(createdAt)
    {
        
    }
    public Guid PartId { get; }

    public int AvailableQuantity { get; }

    public int ReorderQuantity { get; }

    public ReplenishmentStatus Status { get; private set; }

    public void Approve()
    {
        Status = ReplenishmentStatus.Approved;
    }

    public void Reject()
    {
        Status = ReplenishmentStatus.Rejected;
    }
}