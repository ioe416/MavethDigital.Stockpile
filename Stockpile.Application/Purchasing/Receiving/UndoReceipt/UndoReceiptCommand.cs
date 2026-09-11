using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpile.Application.Purchasing.Receiving.UndoReceipt
{
    public sealed record UndoReceiptCommand(
        Guid PurchaseId,
        Guid PurchaseLineId,
        int QuantityToUndo,
        DateTimeOffset CreatedAt);

}
