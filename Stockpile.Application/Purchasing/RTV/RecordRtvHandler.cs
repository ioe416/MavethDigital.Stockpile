
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Purchasing.RTV;
public sealed class RecordRtvHandler
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IRtvRepository _rtvRepository;

    public RecordRtvHandler(
        IPurchaseRepository purchaseRepository,
        IRtvRepository rtvRepository)
    {
        _purchaseRepository = purchaseRepository;
        _rtvRepository = rtvRepository;
    }

    public async Task<RecordRtvResult> HandleAsync(
        RecordRtvCommand command,
        CancellationToken cancellationToken = default)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(command.PurchaseId, cancellationToken);
        if (purchase == null)
            return new RecordRtvResult(false, "Purchase not found.");
        var purchaseLine = purchase.Lines.SingleOrDefault(line => line.Id == command.PurchaseLineId);
        if (purchaseLine == null)
            return new RecordRtvResult(false, "Purchase line not found.");
        
        var rtv = new ReturnToVendor(command.PurchaseId, command.CreatedAt);
        var rtvLine = new RtvLine(command.PurchaseLineId, command.QuantityToReturn, command.CreatedAt);

        rtv.AddLine(command.CreatedAt, rtvLine);

        await _rtvRepository.AddAsync(rtv, cancellationToken);

        purchase.ApplyRtv(command.PurchaseLineId, command.QuantityToReturn, command.CreatedAt);
        
        await _purchaseRepository.UpdateAsync(purchase, cancellationToken);

        return new RecordRtvResult(true, "RTV recorded successfully.", rtv.Id);
        
    }

}
