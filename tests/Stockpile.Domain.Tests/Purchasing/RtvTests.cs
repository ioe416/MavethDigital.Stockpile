using FluentAssertions;
using MavethDigital.Forge.Core.ValueObjects;
using Stockpile.Application.Purchasing.RTV;
using Stockpile.Application.Tests.Fakes;
using Stockpile.Domain.Purchasing.Enums;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Domain.Tests.Purchasing;

public sealed class RtvTests
{
    [Fact]
    public async Task Rtv_should_reduce_received_quantity()
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
        purchase.Order(createdAt.AddMinutes(4), "PO123");

        // Receive 10
        purchase.RecordReceipt(createdAt.AddMinutes(5), line.Id, 10);

        var purchaseRepository = new FakePurchaseRepository { Purchase = purchase };
        var rtvRepository = new FakeRtvRepository();

        var handler = new RecordRtvHandler(purchaseRepository, rtvRepository);

        var rtvCommand = new RecordRtvCommand(
            purchase.Id,
            line.Id,
            3,
            createdAt.AddMinutes(6));

        await handler.HandleAsync(rtvCommand);

        line.ReceivedQuantity.Should().Be(7);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(6));
    }

    [Fact]
    public async Task Rtv_should_reopen_purchase_if_line_becomes_incomplete()
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
        purchase.Order(createdAt.AddMinutes(4), "PO123");

        // Receive full quantity
        purchase.RecordReceipt(createdAt.AddMinutes(5), line.Id, 10);
        purchase.Status.Should().Be(PurchaseStatus.Completed);

        var purchaseRepository = new FakePurchaseRepository { Purchase = purchase };
        var rtvRepository = new FakeRtvRepository();

        var handler = new RecordRtvHandler(purchaseRepository, rtvRepository);

        var rtvCommand = new RecordRtvCommand(
            purchase.Id,
            line.Id,
            2,
            createdAt.AddMinutes(6));

        await handler.HandleAsync(rtvCommand);

        purchase.Status.Should().Be(PurchaseStatus.Ordered);
        line.ReceivedQuantity.Should().Be(8);
    }

    [Fact]
    public async Task Rtv_more_than_received_should_throw()
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
        purchase.Order(createdAt.AddMinutes(4), "PO123");

        // Receive 4
        purchase.RecordReceipt(createdAt.AddMinutes(5), line.Id, 4);

        var purchaseRepository = new FakePurchaseRepository { Purchase = purchase };
        var rtvRepository = new FakeRtvRepository();

        var handler = new RecordRtvHandler(purchaseRepository, rtvRepository);

        var rtvCommand = new RecordRtvCommand(
            purchase.Id,
            line.Id,
            5, // exceeds received
            createdAt.AddMinutes(6));

        Func<Task> act = async () => await handler.HandleAsync(rtvCommand);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot return more than the received quantity.");
    }

    [Fact]
    public async Task Rtv_is_saved_even_when_purchase_rejects()
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
        purchase.Cancel(createdAt.AddMinutes(4)); // invalid status for RTV

        var purchaseRepository = new FakePurchaseRepository { Purchase = purchase };
        var rtvRepository = new FakeRtvRepository();

        var handler = new RecordRtvHandler(purchaseRepository, rtvRepository);

        var rtvCommand = new RecordRtvCommand(
            purchase.Id,
            line.Id,
            3,
            createdAt.AddMinutes(5));

        Func<Task> act = async () => await handler.HandleAsync(rtvCommand);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("RTVs can only be recorded for ordered purchases.");


        rtvRepository.Rtvs.Should().HaveCount(1);
        rtvRepository.AddedRtv!.PurchaseId.Should().Be(purchase.Id);
    }

    [Fact]
    public void Duplicate_rtv_lines_should_throw()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var rtv = new ReturnToVendor(Guid.NewGuid(), createdAt);

        var line = new RtvLine(Guid.NewGuid(), 2, createdAt.AddMinutes(1));

        rtv.AddLine(createdAt.AddMinutes(2), line);

        Action act = () => rtv.AddLine(createdAt.AddMinutes(3), line);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Duplicate lines cannot be added to the same return to vendor.");
    }

    [Fact]
    public void Duplicate_purchaseLineIds_should_throw()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchaseLineId = Guid.NewGuid();

        var rtv = new ReturnToVendor(Guid.NewGuid(), createdAt);

        var line1 = new RtvLine(purchaseLineId, 2, createdAt.AddMinutes(1));
        var line2 = new RtvLine(purchaseLineId, 3, createdAt.AddMinutes(2));

        rtv.AddLine(createdAt.AddMinutes(3), line1);

        Action act = () => rtv.AddLine(createdAt.AddMinutes(4), line2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Duplicate purchase lines cannot be referenced in the same return to vendor.");
    }

    [Fact]
    public void Rtv_timestamp_must_be_monotonic()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var rtv = new ReturnToVendor(Guid.NewGuid(), createdAt);

        var line = new RtvLine(Guid.NewGuid(), 2, createdAt.AddMinutes(1));

        rtv.AddLine(createdAt.AddMinutes(2), line);

        Action act = () => rtv.AddLine(createdAt.AddMinutes(1),
            new RtvLine(Guid.NewGuid(), 1, createdAt.AddMinutes(1)));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The changed timestamp cannot precede the current updated timestamp.*");
    }

    [Fact]
    public void Purchase_should_reopen_when_any_completed_line_is_returned_to_vendorte()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);
        var line1 = new PurchaseLine(
            Guid.NewGuid(),
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));
        var line2 = new PurchaseLine(
            Guid.NewGuid(),
            5,
            0,
            createdAt.AddMinutes(2),
            new Money(5.00m, new CurrencyCode("USD")));
        purchase.AddLine(createdAt.AddMinutes(3), line1);
        purchase.AddLine(createdAt.AddMinutes(4), line2);
        purchase.Submit(createdAt.AddMinutes(5));
        purchase.Order(createdAt.AddMinutes(6), "PO123");
        // Receive full quantity for both lines
        purchase.RecordReceipt(createdAt.AddMinutes(7), line1.Id, 10);
        purchase.RecordReceipt(createdAt.AddMinutes(8), line2.Id, 5);
        purchase.Status.Should().Be(PurchaseStatus.Completed);
        // Return some quantity for both lines
        purchase.ApplyRtv(line1.Id, 2, createdAt.AddMinutes(9));
        line1.ReceivedQuantity.Should().Be(8);
        line2.ReceivedQuantity.Should().Be(5);
        // After RTVs, the purchase should be reopened
        purchase.Status.Should().Be(PurchaseStatus.Ordered);
    }

    [Fact]
    public void Purchase_should_not_complete_until_all_lines_are_fully_received()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);
        var line1 = new PurchaseLine(
            Guid.NewGuid(),
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));
        var line2 = new PurchaseLine(
            Guid.NewGuid(),
            5,
            0,
            createdAt.AddMinutes(2),
            new Money(5.00m, new CurrencyCode("USD")));
        purchase.AddLine(createdAt.AddMinutes(3), line1);
        purchase.AddLine(createdAt.AddMinutes(4), line2);
        purchase.Submit(createdAt.AddMinutes(5));
        purchase.Order(createdAt.AddMinutes(6), "PO123");
        // Receive full quantity for line1 and partial for line2
        purchase.RecordReceipt(createdAt.AddMinutes(7), line1.Id, 10);
        purchase.RecordReceipt(createdAt.AddMinutes(8), line2.Id, 4);
        // The purchase should not be completed yet
        line1.ReceivedQuantity.Should().Be(10);
        line2.ReceivedQuantity.Should().Be(4);
        line1.OutstandingQuantity.Should().Be(0);
        line2.OutstandingQuantity.Should().Be(1);
        purchase.Status.Should().Be(PurchaseStatus.Ordered);
    }

    [Fact]
    public void Purchase_should_reopen_when_undoing_receipt_makes_a_line_incomplete()
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
        purchase.Order(createdAt.AddMinutes(4), "PO123");
        // Receive full quantity
        purchase.RecordReceipt(createdAt.AddMinutes(5), line.Id, 10);
        purchase.Status.Should().Be(PurchaseStatus.Completed);
        // Undo receipt of 5 units
        purchase.UndoReceipt(line.Id, 5, createdAt.AddMinutes(6));
        // After undoing receipt, the purchase should be reopened
        purchase.Status.Should().Be(PurchaseStatus.Ordered);
        line.ReceivedQuantity.Should().Be(5);
        line.OutstandingQuantity.Should().Be(5);
    }

}
