
using MavethDigital.Forge.Domain.Models;

namespace Stockpile.Domain.Purchasing.Models;

public sealed class ReturnToVendor : AggregateRoot
{
    public Guid PurchaseId { get; }

    private readonly List<RtvLine> _lines = new();

    public IReadOnlyCollection<RtvLine> Lines => _lines.AsReadOnly();

    public ReturnToVendor(Guid purchaseId, DateTimeOffset createdAt)
        : base(createdAt)
    {
        if (purchaseId == Guid.Empty)
            throw new ArgumentException("Return to vendor cannot contain an empty purchase", nameof(PurchaseId));
    
        PurchaseId = purchaseId;
    }

    public void AddLine(DateTimeOffset updatedAt, RtvLine line)
    {
        if (line == null)
            throw new ArgumentNullException(nameof(line));
        if (_lines.Any(x => x.Id == line.Id))
            throw new InvalidOperationException("Duplicate lines cannot be added to the same return to vendor.");
        if (_lines.Any(x => x.PurchaseLineId == line.PurchaseLineId))
            throw new InvalidOperationException("Duplicate purchase lines cannot be referenced in the same return to vendor.");
        if (updatedAt < UpdatedAt)
            throw new InvalidOperationException("The changed timestamp cannot precede the current updated timestamp.");

        base.MarkUpdated(updatedAt);

        _lines.Add(line);
    }
}
