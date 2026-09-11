using MavethDigital.Forge.Core.ValueObjects;
using MavethDigital.Forge.Domain.Models;
using Stockpile.Domain.Purchasing.Enums;

namespace Stockpile.Domain.Purchasing.Models;

public sealed class Purchase : AggregateRoot
{
    private readonly List<PurchaseLine> _lines = [];
    public Guid VendorId { get; }
    
    public Guid DepartmentId { get; }

    public Guid RequestedByEmployeeId { get; }

    public PurchaseStatus Status { get; private set; }

    public string? PurchaseOrderNumber { get; private set; }

    public IReadOnlyCollection<PurchaseLine> Lines => _lines;

    public Purchase(Guid vendorId, 
        Guid departmentId, 
        Guid requestedByEmployeeId,
        DateTimeOffset createdAt,
        string? purchaseOrderNumber = null) 
        : base (createdAt)
    {
        if (vendorId == Guid.Empty)
            throw new ArgumentException("A vendor is required.", nameof(vendorId));

        if (departmentId == Guid.Empty)
            throw new ArgumentException("A department is required.", nameof(departmentId));

        if (requestedByEmployeeId == Guid.Empty)
            throw new ArgumentException("A requesting employee is required.", nameof(requestedByEmployeeId));

        VendorId = vendorId;
        DepartmentId = departmentId;
        RequestedByEmployeeId = requestedByEmployeeId;
        PurchaseOrderNumber = purchaseOrderNumber;

        Status = PurchaseStatus.Draft;
    }

    public void Submit(DateTimeOffset submittedAt)
    {
        if (Status != PurchaseStatus.Draft)
            throw new InvalidOperationException("Only draft purchases can be submitted.");

        base.MarkUpdated(submittedAt);
        Status = PurchaseStatus.Requested;
    }

    public void Order(DateTimeOffset orderedAt, string poNumber)
    {

        if (Status != PurchaseStatus.Requested)
            throw new InvalidOperationException("Only requested purchases can be ordered.");

        if (string.IsNullOrWhiteSpace(poNumber))
            throw new ArgumentException("A purchase order number is required.", nameof(poNumber));

        base.MarkUpdated(orderedAt);

        PurchaseOrderNumber = poNumber.Trim();
        Status = PurchaseStatus.Ordered;
    }

    public void AddLine(DateTimeOffset updatedAt, PurchaseLine line)
    {
        if (line is null)
            throw new ArgumentNullException(nameof(line));

        if (Status == PurchaseStatus.Ordered)
            throw new InvalidOperationException("Purchase lines cannot be added to an ordered purchase.");

        if (Status == PurchaseStatus.Cancelled)
            throw new InvalidOperationException("Purchase lines cannot be added to a cancelled purchase.");

        if (_lines.Any(x => x.Id == line.Id))
            throw new InvalidOperationException("Duplicate lines cannot be added to the same purchase.");
            
        base.MarkUpdated(updatedAt);

        _lines.Add(line);
        
    }

    public void RemoveLine(DateTimeOffset updatedAt, Guid lineId)
    {
        if (lineId == Guid.Empty)
            throw new ArgumentException(
                "A valid purchase line must be selected.",
                nameof(lineId));

        var existingLine = _lines.SingleOrDefault(x => x.Id == lineId);

        if (Status == PurchaseStatus.Ordered)
            throw new InvalidOperationException("Purchase lines cannot be removed from an ordered purchase.");

        if (Status == PurchaseStatus.Cancelled)
            throw new InvalidOperationException("Purchase lines cannot be removed from a cancelled purchase.");

        if (existingLine is null)
            throw new InvalidOperationException("Purchase does not contain the selected line");

        base.MarkUpdated(updatedAt);

        _lines.Remove(existingLine);

    }

    public void UpdateQuantity(
        DateTimeOffset updatedAt, 
        Guid lineId, 
        int newQuantity)
    {
        var existingLine = _lines.SingleOrDefault(x => x.Id == lineId);

        if (existingLine is null)
            throw new ArgumentException("A valid purchase line must be selected",
                nameof(lineId));

        if (Status == PurchaseStatus.Ordered)
            throw new InvalidOperationException("Quantity cannot be altered on an ordered purchase.");

        if (Status == PurchaseStatus.Cancelled)
            throw new InvalidOperationException("Quantity cannot be altered on a cancelled purchase.");

        if (updatedAt < UpdatedAt)
            throw new ArgumentOutOfRangeException("Purchase line updates cannot pre-date purchase updates");

        if (newQuantity < 1)
            throw new ArgumentOutOfRangeException(
                "A valid positive quantity is required");

        existingLine.UpdateQuantity(updatedAt, newQuantity);

        MarkUpdated(updatedAt);

    }

    public void UpdateUnitPrice(
        DateTimeOffset updatedAt,
        Guid lineId,
        Money? newUnitPrice)
    {
        var existingLine = _lines.SingleOrDefault(x => x.Id == lineId);

        if (existingLine is null)
            throw new ArgumentException("A valid purchase line must be selected",
                nameof(lineId));

        if (Status == PurchaseStatus.Ordered)
            throw new InvalidOperationException("Unit Price cannot be altered on an ordered purchase.");

        if (Status == PurchaseStatus.Cancelled)
            throw new InvalidOperationException("Unit Price cannot be altered on a cancelled purchase.");

        if (updatedAt < UpdatedAt)
            throw new ArgumentOutOfRangeException("Purchase line updates cannot pre-date purchase updates");

        if (newUnitPrice == null)
            throw new ArgumentException(
                "A valid positive unit price is required");

        existingLine.UpdateUnitPrice(updatedAt, newUnitPrice);

        MarkUpdated(updatedAt);

    }

    public void UpdatePartId(
        DateTimeOffset updatedAt,
        Guid lineId,
        Guid newPartId)
    {
        var existingLine = _lines.SingleOrDefault(x => x.Id == lineId);

        if (existingLine is null)
            throw new ArgumentException("A valid purchase line must be selected",
                nameof(lineId));

        if (Status == PurchaseStatus.Ordered)
            throw new InvalidOperationException("Part cannot be altered on an ordered purchase.");

        if (Status == PurchaseStatus.Cancelled)
            throw new InvalidOperationException("Part cannot be altered on a cancelled purchase.");

        if (updatedAt < UpdatedAt)
            throw new ArgumentOutOfRangeException("Purchase line updates cannot pre-date purchase updates");

        if (newPartId == Guid.Empty)
            throw new ArgumentException(
                "A valid part is required");

        existingLine.UpdatePartId(updatedAt, newPartId);

        base.MarkUpdated(updatedAt);

    }

    public void Cancel(DateTimeOffset updatedAt)
    {
        if (Status == PurchaseStatus.Ordered)
            throw new InvalidOperationException("An ordered purchase cannot be cancelled.");

        if (Status == PurchaseStatus.Cancelled)
            throw new InvalidOperationException("A cancelled purchase cannot be cancelled.");

        base.MarkUpdated(updatedAt);

        Status = PurchaseStatus.Cancelled;
    }

    public void RecordReceipt(DateTimeOffset updatedAt, Guid lineId, int receivedQuantity)
    {
        var existingLine = _lines.SingleOrDefault(x => x.Id == lineId);
        if (existingLine is null)
            throw new ArgumentException("A valid purchase line must be selected",
                nameof(lineId));
        if (Status != PurchaseStatus.Ordered)
            throw new InvalidOperationException("Receipts can only be recorded for ordered purchases.");
        if (updatedAt < UpdatedAt)
            throw new ArgumentOutOfRangeException("Purchase line updates cannot pre-date purchase updates");
        
        existingLine.UpdateReceivedQuantity(updatedAt, receivedQuantity);

        if (_lines.All(x => x.ReceivedQuantity == x.Quantity))
        {
            Status = PurchaseStatus.Completed;
        }

        MarkUpdated(updatedAt);
    }

    public void UndoReceipt(Guid lineId, int quantity, DateTimeOffset updatedAt)
    {
        var existingLine = _lines.SingleOrDefault(x => x.Id == lineId);

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        if (existingLine is null)
            throw new ArgumentException("A valid receipt line must be selected",
                nameof(lineId));

        if (quantity > existingLine.ReceivedQuantity)
            throw new InvalidOperationException("Cannot undo more than the received quantity.");


        existingLine.ReceivedQuantity -= quantity;

        base.MarkUpdated(updatedAt);

        if (_lines.Any(x => x.ReceivedQuantity < x.Quantity))
            Status = PurchaseStatus.Ordered;
    }
}
