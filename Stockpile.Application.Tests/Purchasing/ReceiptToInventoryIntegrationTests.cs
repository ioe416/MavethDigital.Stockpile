using FluentAssertions;
using MavethDigital.Forge.Core.ValueObjects;
using Stockpile.Application.Purchasing.Receiving.RecordReceipt;
using Stockpile.Application.Tests.Fakes;
using Stockpile.Domain.Inventory.Enums;
using Stockpile.Domain.Inventory.Models;
using Stockpile.Domain.Purchasing.Enums;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Tests.Purchasing;

public sealed class ReceiptToInventoryIntegrationTests
{
    [Fact]
    public async Task Recording_a_receipt_should_increase_inventory()
    {
        // Arrange

        var createdAt = DateTimeOffset.UtcNow;

        var vendorId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var partId = Guid.NewGuid();

        var purchase = new Purchase(
            vendorId,
            warehouseId,
            userId,
            createdAt);

        var purchaseLine = new PurchaseLine(
            partId,
            quantity: 10,
            receivedQuantity: 0,
            unitPrice: new Money(
                5m,
                new CurrencyCode("USD")),
            createdAt: createdAt);

        purchase.AddLine(
            createdAt.AddMinutes(1),
            purchaseLine);

        purchase.Submit(
            createdAt.AddMinutes(2));

        purchase.Order(
            createdAt.AddMinutes(3),
            "PO-123");

        var purchaseRepository =
            new FakePurchaseRepository();

        await purchaseRepository.UpdateAsync(
            purchase,
            CancellationToken.None);

        var receiptRepository =
            new FakeReceiptRepository();

        var inventory = new InventoryItem(
            partId,
            onHandQuantity: 0,
            allocatedQuantity: 0,
            minimumQuantity: 5,
            reorderQuantity: 10,
            createdAt);

        var inventoryItems = new List<InventoryItem>
        {
            inventory
        };

        var inventoryRepository =
            new FakeInventoryRepository(inventoryItems);

        var handler =
            new RecordReceiptHandler(
                purchaseRepository,
                receiptRepository,
                inventoryRepository);

        var command =
            new RecordReceiptCommand(
                PurchaseId: purchase.Id,
                PurchaseLineId: purchaseLine.Id,
                QuantityReceived: 10,
                CreatedAt: createdAt.AddMinutes(4));

        // Act

        await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert

        purchase.Lines.Single()
            .ReceivedQuantity.Should()
            .Be(10);

        purchase.Status.Should()
            .Be(PurchaseStatus.Completed);

        inventory.OnHandQuantity.Should()
            .Be(10);

        inventory.TransactionHistory
            .Should()
            .ContainSingle(x =>
                x.TransactionType ==
                TransactionType.Receipt);

        inventory.TransactionHistory
            .Single()
            .Quantity
            .Should()
            .Be(10);
    }
}