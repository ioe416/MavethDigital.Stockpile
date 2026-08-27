using MavethDigital.Forge.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpile.Domain.Purchasing.Models
{
    public sealed class Receipt : AggregateRoot
    {
        private readonly List<ReceiptLine> _lines = [];

        public Guid PurchaseId { get; }

        public Receipt(
            Guid purchaseId,
            DateTimeOffset createdAt) 
            : base (createdAt)
        {
            if (purchaseId == Guid.Empty)
                throw new InvalidOperationException("Receipt cannot contain an empty purchase");

            PurchaseId = purchaseId;
        }
    }
}
