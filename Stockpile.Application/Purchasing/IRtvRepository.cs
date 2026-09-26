
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Purchasing;

public interface IRtvRepository
{
    public Task AddAsync(
        ReturnToVendor rtv,
        CancellationToken cancellationToken = default);
}
