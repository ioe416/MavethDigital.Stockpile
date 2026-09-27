
using MavethDigital.Forge.Core.Models;
using Stockpile.Domain.Inventory.Enums;

namespace Stockpile.Domain.Inventory.Models;

public sealed class InventoryTransaction : Entity
{
    public TransactionType TransactionType { get; private set; }

    public int Quantity { get; private set; }

    public InventoryTransaction(
        TransactionType transactionType, 
        int quantity,
        DateTimeOffset createdAt)
        : base(createdAt)
    {
        TransactionType = transactionType;
        Quantity = quantity;
    }
}
