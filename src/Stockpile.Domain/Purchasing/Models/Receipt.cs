using MavethDigital.Forge.Domain.Models;
using Stockpile.Domain.Purchasing.Enums;
using System.Net.NetworkInformation;


namespace Stockpile.Domain.Purchasing.Models;

public sealed class Receipt : AggregateRoot
{
    private readonly List<ReceiptLine> _lines = [];

    public Guid PurchaseId { get; }

    public IReadOnlyCollection<ReceiptLine> Lines => _lines;

    public Receipt(
        Guid purchaseId,
        DateTimeOffset createdAt) 
        : base (createdAt)
    {
        if (purchaseId == Guid.Empty)
            throw new ArgumentException("Receipt cannot contain an empty purchase", nameof(purchaseId));

        PurchaseId = purchaseId;
    }

    public void AddLine(
        DateTimeOffset updatedAt, 
        ReceiptLine line)
    {
        if (line == null) 
            throw new ArgumentNullException(nameof(line));

        if (_lines.Any(x => x.Id == line.Id))
            throw new InvalidOperationException(
                "Duplicate lines cannot be added to the same receipt.");

        base.MarkUpdated(updatedAt);

        _lines.Add(line);

    }
}
