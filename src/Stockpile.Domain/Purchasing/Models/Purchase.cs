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

        base.MarkUpdated(updatedAt);

        _lines.Add(line);
        
    }

    public void UpdateQuantity(
        DateTimeOffset updatedAt, 
        Guid lineId, 
        int newQuantity)
    {
        var existingLine = _lines.SingleOrDefault(x => x.Id == lineId);

        if (existingLine is null)
            throw new ArgumentException("A valid purchase lineId must be selected",
                nameof(existingLine));

        if (Status == PurchaseStatus.Ordered)
            throw new InvalidOperationException("Quantity cannot be altered on an ordered purchase.");

        if (Status == PurchaseStatus.Cancelled)
            throw new InvalidOperationException("Quantity cannot be altered on a cancelled purchase.");

        existingLine.UpdateQuantity(updatedAt, newQuantity);

        MarkUpdated(updatedAt);

    }

    public void UpdateUnitPrice(
        DateTimeOffset updatedAt,
        Guid lineId,
        Money newUnitPrice)
    {
        var existingLine = _lines.SingleOrDefault(x => x.Id == lineId);

        if (existingLine is null)
            throw new ArgumentException("A valid purchase line must be selected",
                nameof(existingLine));

        if (Status == PurchaseStatus.Ordered)
            throw new InvalidOperationException("Unit Price cannot be altered on an ordered purchase.");

        if (Status == PurchaseStatus.Cancelled)
            throw new InvalidOperationException("Unit Price cannot be altered on a cancelled purchase.");

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
                nameof(existingLine));

        if (Status == PurchaseStatus.Ordered)
            throw new InvalidOperationException("Part cannot be altered on an ordered purchase.");

        if (Status == PurchaseStatus.Cancelled)
            throw new InvalidOperationException("Part cannot be altered on a cancelled purchase.");

        if (base.UpdatedAt > existingLine.UpdatedAt)
            throw new ArgumentException("Purchase line updates cannot pre-date purchase updates");


        existingLine.UpdatePartId(updatedAt, newPartId);


        MarkUpdated(updatedAt);

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
}
