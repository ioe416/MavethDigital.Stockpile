using MavethDigital.Forge.Domain.Models;
using Stockpile.Domain.Inventory.Enums;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Domain.Inventory.Models;

public sealed class InventoryItem : AggregateRoot
{
    private readonly List<InventoryTransaction> _transactionHistory = [];
    public Guid PartId { get; private set; }

    public int OnHandQuantity { get; private set; }

    public IReadOnlyCollection<InventoryTransaction> TransactionHistory => _transactionHistory;

    public InventoryItem(
        Guid partId,
        int onHandQuantity,
        DateTimeOffset createdAt)
        : base(createdAt)
    {
        PartId = partId;
        OnHandQuantity = onHandQuantity;
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

    public void Issue(
        int quantity,
        DateTimeOffset createdAt)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        OnHandQuantity -= quantity;
        _transactionHistory.Add(new InventoryTransaction(TransactionType.Issue, quantity, createdAt));
    }
}
