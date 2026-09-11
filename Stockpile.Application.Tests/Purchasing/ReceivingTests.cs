
using FluentAssertions;
using MavethDigital.Forge.Core.ValueObjects;
using Stockpile.Application.Purchasing.Receiving.RecordReceipt;
using Stockpile.Application.Tests.Fakes;
using Stockpile.Domain.Purchasing.Enums;
using Stockpile.Domain.Purchasing.Models;
using System.Diagnostics;

namespace Stockpile.Application.Tests.Purchasing;

public sealed class ReceivingTests
{
    [Fact]
    public async Task Receipt_with_valid_Guid_should_record()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "123456");

        var purchaseRepository = new FakePurchaseRepository
        {
            Purchase = purchase
        };

        var receiptRepository = new FakeReceiptRepository { };

        var handler = new RecordReceiptHandler(
            purchaseRepository,
            receiptRepository);

        var command = new RecordReceiptCommand(
            purchase.Id,
            line.Id,
            4,
            createdAt.AddMinutes(5));

        await handler.HandleAsync(command);

        var receipt = receiptRepository.AddedReceipt;

        receipt.Should().NotBeNull();
        receipt!.PurchaseId.Should().Be(purchase.Id);
        receipt.Lines.Should().HaveCount(1);

        var receiptLine = receipt.Lines.Single();

        receiptLine.PurchaseLineId.Should().Be(line.Id);
        receiptLine.QuantityReceived.Should().Be(4);

    }

    [Fact]
    public async Task Receipt_for_purchase_that_does_not_exist_should_fail_and_not_persist_a_receipt()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchaseRepository = new FakePurchaseRepository
        {
            Purchase = null
        };
        var receiptRepository = new FakeReceiptRepository { };

        var handler = new RecordReceiptHandler(
            purchaseRepository,
            receiptRepository);

        var command = new RecordReceiptCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            4,
            createdAt.AddMinutes(5));

        Func<Task> act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Purchase not found");

        receiptRepository.AddedReceipt.Should().BeNull();
    }

    [Fact]
    public async Task Purchase_that_does_not_contain_purchaseLine_should_fail_and_throw_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
                Guid.NewGuid(),
                10,
                0,
                createdAt.AddMinutes(1),
                new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "123456");

        var purchaseRepository = new FakePurchaseRepository
        {
            Purchase = purchase
        };

        var receiptRepository = new FakeReceiptRepository { };

        var missingLineId = Guid.NewGuid(); // This ID does not exist in the purchase lines

        var handler = new RecordReceiptHandler(
            purchaseRepository,
            receiptRepository);

        var command = new RecordReceiptCommand(
            purchase.Id,
            missingLineId,
            4,
            createdAt.AddMinutes(5));

        Func<Task> act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Purchase line not found");

        receiptRepository.AddedReceipt.Should().BeNull();
    }

    [Fact]
    public async Task A_valid_receipt_where_received_quantity_does_not_exceed_ordered_quantity_should_record()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "123456");

        var purchaseRepository = new FakePurchaseRepository
        {
            Purchase = purchase
        };
        var receiptRepository = new FakeReceiptRepository { };
        var handler = new RecordReceiptHandler(
            purchaseRepository,
            receiptRepository);
        var command = new RecordReceiptCommand(
            purchase.Id,
            line.Id,
            5, // Received quantity is less than ordered quantity
            createdAt.AddMinutes(5));

        await handler.HandleAsync(command);
        var receipt = receiptRepository.AddedReceipt;
        receipt.Should().NotBeNull();
        receipt!.PurchaseId.Should().Be(purchase.Id);
        receipt.Lines.Should().HaveCount(1);
        var receiptLine = receipt.Lines.Single();
        receiptLine.PurchaseLineId.Should().Be(line.Id);
        receiptLine.QuantityReceived.Should().Be(5);
    }

    [Fact]
    public async Task When_a_valid_receivedQauntity_is_recorded_purchaseLines_receivedQuantity_should_update()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);
        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));
        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "123456");
        var purchaseRepository = new FakePurchaseRepository
        {
            Purchase = purchase
        };
        var receiptRepository = new FakeReceiptRepository { };
        var handler = new RecordReceiptHandler(
            purchaseRepository,
            receiptRepository);
        var command = new RecordReceiptCommand(
            purchase.Id,
            line.Id,
            4,
            createdAt.AddMinutes(5));
        await handler.HandleAsync(command);

        line.ReceivedQuantity.Should().Be(4);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(5));
    }

    [Fact]
    public async Task A_receivedQauntity_greater_than_ordered_quantity_should_fail_to_update_and_throw_an_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);
        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));
        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "123456");
        var purchaseRepository = new FakePurchaseRepository
        {
            Purchase = purchase
        };
        var receiptRepository = new FakeReceiptRepository { };
        var handler = new RecordReceiptHandler(
            purchaseRepository,
            receiptRepository);
        var command = new RecordReceiptCommand(
            purchase.Id,
            line.Id,
            11,
            createdAt.AddMinutes(5));

        Func<Task> act = async () => await handler.HandleAsync(command);

        Console.WriteLine($"Purchase Ordered Quantity: {line.Quantity}, " +
            $"Attempted Received Quantity: {command.QuantityReceived}");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Total Received quantity cannot exceed ordered quantity*");

        line.ReceivedQuantity.Should().Be(0);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
    }

    [Fact]
    public async Task Multiple_receipts_accumulate()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);
        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));
        
        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "123456");
        
        var purchaseRepository = new FakePurchaseRepository
        {
            Purchase = purchase
        };
        var receiptRepository = new FakeReceiptRepository { };
        var handler = new RecordReceiptHandler(
            purchaseRepository,
            receiptRepository);

        var receipt1 = new RecordReceiptCommand(
            purchase.Id,
            line.Id,
            4,
            createdAt.AddMinutes(5));

        var receipt2 = new RecordReceiptCommand(
            purchase.Id,
            line.Id,
            3,
            createdAt.AddMinutes(6));

        await handler.HandleAsync(receipt1);

        await handler.HandleAsync(receipt2);

        line.ReceivedQuantity.Should().Be(7);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(6));
    }

    [Fact]
    public async Task Receiving_the_exact_quantity_ordered_should_complete_the_order()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);
        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "123456");

        var purchaseRepository = new FakePurchaseRepository
        {
            Purchase = purchase
        };
        var receiptRepository = new FakeReceiptRepository { };
        var handler = new RecordReceiptHandler(
            purchaseRepository,
            receiptRepository);

        var receipt1 = new RecordReceiptCommand(
            purchase.Id,
            line.Id,
            10,
            createdAt.AddMinutes(5));

        await handler.HandleAsync(receipt1);

        line.ReceivedQuantity.Should().Be(10);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(5));
        purchase.Status.Should().Be(PurchaseStatus.Completed);
    }

    [Fact]
    public async Task Accumulating_the_exact_quantity_ordered_should_complete_the_order()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);
        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "123456");

        var purchaseRepository = new FakePurchaseRepository
        {
            Purchase = purchase
        };
        var receiptRepository = new FakeReceiptRepository { };
        var handler = new RecordReceiptHandler(
            purchaseRepository,
            receiptRepository);

        var receipt1 = new RecordReceiptCommand(
            purchase.Id,
            line.Id,
            4,
            createdAt.AddMinutes(5));

        var receipt2 = new RecordReceiptCommand(
            purchase.Id,
            line.Id,
            6,
            createdAt.AddMinutes(6));

        await handler.HandleAsync(receipt1);
        await handler.HandleAsync(receipt2);

        line.ReceivedQuantity.Should().Be(10);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(6));
        purchase.Status.Should().Be(PurchaseStatus.Completed);
    }
}