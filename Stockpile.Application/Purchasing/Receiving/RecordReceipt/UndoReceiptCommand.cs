using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpile.Application.Purchasing.Receiving.RecordReceipt
{
    public sealed record UndoReceiptCommand(
        Guid PurchaseId,
        Guid PurchaseLineId,
        int QuantityToUndo,
        DateTimeOffset CreatedAt);

}
