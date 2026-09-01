
using FluentAssertions;
using MavethDigital.Forge.Core.ValueObjects;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Tests.Purchasing.Receiving;

public sealed class ReceivingTests
{
    [Fact]
    public void Receipt_with_valid_Guid_should_record()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchaseId = Guid.NewGuid();
        var purchaseLineId = Guid.NewGuid();

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);
        
        purchase.Submit(createdAt.AddMinutes(3));

        purchase.Order(createdAt.AddMinutes(4), "123456");

        var receipt = new Receipt(
            purchaseId,
            createdAt.AddMinutes(5));

        var receiptLine = new ReceiptLine(
            purchaseLineId,
            4,
            createdAt.AddMinutes(6));

        receipt.PurchaseId.Should().Be(purchaseId);
        receiptLine.PurchaseLineId.Should().Be(purchaseLineId);
        receiptLine.QuantityReceived.Should().Be(4);

    }
}
