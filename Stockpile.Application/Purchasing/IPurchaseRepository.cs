using Stockpile.Domain.Purchasing.Models;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace Stockpile.Application.Purchasing
{
    public interface IPurchaseRepository
    {
        Task<Purchase?> GetByIdAsync(
        Guid purchaseId,
        CancellationToken cancellationToken = default);

    }
}
