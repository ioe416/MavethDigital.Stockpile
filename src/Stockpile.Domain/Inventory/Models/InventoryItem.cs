using MavethDigital.Forge.Domain.Models;
using Stockpile.Domain.Inventory.Enums;

namespace Stockpile.Domain.Inventory.Models;

public sealed class InventoryItem : AggregateRoot
{
    private readonly List<InventoryTransaction> _transactionHistory = [];
    public Guid PartId { get; private set; }

    public int OnHandQuantity { get; private set; }

    public int AllocatedQuantity { get; private set; }

    public int AvailableQuantity => OnHandQuantity - AllocatedQuantity;

    public int MinimumQuantity { get; private set; }

    public int ReorderQuantity { get; private set; }

    public bool NeedsReplenishment => AvailableQuantity < MinimumQuantity;

    public IReadOnlyCollection<InventoryTransaction> TransactionHistory => _transactionHistory;

    public InventoryItem(
        Guid partId,
        int onHandQuantity,
        int allocatedQuantity,
        int minimumQuantity,
        int reorderQuantity,
        DateTimeOffset createdAt)
        : base(createdAt)
    {
        PartId = partId;
        OnHandQuantity = onHandQuantity;
        AllocatedQuantity = allocatedQuantity;
        MinimumQuantity = minimumQuantity;
        ReorderQuantity = reorderQuantity;
    }

    public void Receive(
        int quantity, 
        DateTimeOffset createdAt)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        OnHandQuantity += quantity;
        _transactionHistory.Add(new InventoryTransaction(TransactionType.Receipt, quantity, createdAt));
    }

    public void IssueAllocated(
        int quantity,
        DateTimeOffset createdAt)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (quantity > AllocatedQuantity)
        {
            throw new InvalidOperationException(
            "Cannot issue more than allocated quantity.");
        }

        OnHandQuantity -= quantity;
        AllocatedQuantity -= quantity;
        _transactionHistory.Add(new InventoryTransaction(TransactionType.Issue, quantity, createdAt));
    }

    public void IssueUnallocated(
        int quantity,
        DateTimeOffset createdAt)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (quantity > AvailableQuantity)
        {
            // TODO: Develop code to prompt a re-order for the
            // part if the available quantity is insufficient.
        }

        OnHandQuantity -= quantity;
        _transactionHistory.Add(new InventoryTransaction(TransactionType.Issue, quantity, createdAt));
    }

    public void Allocate(
        int quantity,
        DateTimeOffset createdAt)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        AllocatedQuantity += quantity;
        _transactionHistory.Add(new InventoryTransaction(TransactionType.Allocate, quantity, createdAt));
    }

    public void Deallocate(
        int quantity,
        DateTimeOffset createdAt)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (quantity > AllocatedQuantity)
        {
            throw new InvalidOperationException("Cannot deallocate more than the allocated quantity.");
        }


        AllocatedQuantity -= quantity;

        _transactionHistory.Add(new InventoryTransaction(TransactionType.Deallocate, quantity, createdAt));
    }
}
