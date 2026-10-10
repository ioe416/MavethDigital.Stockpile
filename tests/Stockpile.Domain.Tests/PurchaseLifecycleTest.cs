using FluentAssertions;
using MavethDigital.Forge.Core.ValueObjects;
using Stockpile.Domain.Purchasing.Enums;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Domain.Tests;

public sealed class PurchaseLifecycleTest
{
    [Fact]
    public void Purchase_lifecycle_should_support_receiving_and_completion()
    {
        // Arrange

        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt: createdAt);

        var partId = Guid.NewGuid();

        var purchaseLine = new PurchaseLine(
            partId,
            quantity: 10,
            createdAt: createdAt.AddMinutes(1),
            receivedQuantity: 0,
            unitPrice: new Money(5m, new CurrencyCode("USD")));

        purchase.AddLine(
            createdAt.AddMinutes(2),
            purchaseLine);

        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "123456");

        // Receive

        purchase.RecordReceipt(
            createdAt.AddMinutes(5),
            purchaseLine.Id,
            receivedQuantity: 10);

        // Assert

        purchase.Status.Should()
            .Be(PurchaseStatus.Completed);

        purchase.Lines.Single()
            .ReceivedQuantity.Should()
            .Be(10);
    }
}
