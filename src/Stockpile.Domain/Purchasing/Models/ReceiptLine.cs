using MavethDigital.Forge.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpile.Domain.Purchasing.Models
{
    public sealed class ReceiptLine : Entity
    {
        public ReceiptLine(
            DateTimeOffset createdAt) 
            : base (createdAt)
        {
            
        }
    }
}
