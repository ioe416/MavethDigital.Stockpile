using MavethDigital.Forge.Domain.Models;
using Stockpile.Domain.Purchasing.Enums;

namespace Stockpile.Domain.Purchasing.Models;

public sealed class Purchase : AggregateRoot
{
    public Guid VendorId { get; }
    
    public Guid DepartmentId { get; }

    public Guid RequestedByEmployeeId { get; }

    public PurchaseStatus Status { get; private set; }

    public string? PurchaseOrderNumber { get; private set; }

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
}
