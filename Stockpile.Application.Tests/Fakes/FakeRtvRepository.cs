using Stockpile.Application.Purchasing;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Tests.Fakes;

public sealed class FakeRtvRepository 
    : IRtvRepository
{
    private readonly List<ReturnToVendor> _rtvs = new List<ReturnToVendor>();

    public IReadOnlyCollection<ReturnToVendor> Rtvs => _rtvs.AsReadOnly();

    public ReturnToVendor? AddedRtv { get; private set; }

    public Task AddAsync(
        ReturnToVendor rtv,
        CancellationToken cancellationToken = default)
    {
        AddedRtv = rtv;
        _rtvs.Add(rtv);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        ReturnToVendor rtv,
        CancellationToken cancellationToken = default)
    {
        var existingRtv = _rtvs.SingleOrDefault(x => x.Id == rtv.Id);
        if (existingRtv != null)
        {
            _rtvs.Remove(existingRtv);
            _rtvs.Add(rtv);
        }
        return Task.CompletedTask;
    }

    public Task<ReturnToVendor?> GetByIdAsync(
            Guid rtvId,
            CancellationToken cancellationToken = default)
    {
        var rtv = _rtvs.SingleOrDefault(x => x.Id == rtvId);
        return Task.FromResult(rtv);
    }

    public Task<ReturnToVendor?> GetByPurchaseIdAsync(
        Guid purchaseId,
        CancellationToken cancellationToken = default)
    {
        var rtv = _rtvs.SingleOrDefault(x => x.PurchaseId == purchaseId);
        return Task.FromResult(rtv);
    }

}
